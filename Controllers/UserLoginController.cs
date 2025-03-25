using System;
using System.Linq;
using Contoso.Data;
using Contoso.Models;
using Microsoft.AspNetCore.Mvc;
using Contoso.Helpers;  // Add the PasswordHasherService reference here
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Contoso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly PasswordHasherService _passwordHasherService;  // Inject PasswordHasherService
        private readonly IConfiguration _configuration; // For JWT configuration

        public UserLoginController(SchoolContext context, PasswordHasherService passwordHasherService, IConfiguration configuration)
        {
            _context = context;
            _passwordHasherService = passwordHasherService;
            _configuration = configuration;
        }

        // POST: api/userlogin/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validate request
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Invalid email or password.");
            }

            // Find the user by email
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Validate password using the PasswordHasherService
            var isPasswordValid = _passwordHasherService.VerifyPassword(user.Password, request.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid password.");
            }

            var token = GenerateJwtToken(user);

            // Return token along with user information
            return Ok(new { Token = token, Email = user.Email, FullName = user.FullName, Password = user.Password });
        }


        // GET: api/userlogin/{email}
        [HttpGet("{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                user.DepartmentID
            });
        }

        // GET: api/userlogin/login?email=nejc.severmihelic@scv.si&password=Evx92iDLhVSaUi6TuzS3QnqeVwPcFW6dK3/qghPlBE0=
        [HttpGet("login")]
        public IActionResult GetLogin([FromQuery] string email, [FromQuery] string password)
        {
            // Validate email and password
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Email and password are required.");
            }

            // Check if user exists in the database (case-insensitive comparison)
            var user = _context.Users
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            /*Console.WriteLine($"Received password: {password}");
            Console.WriteLine($"Database password: {user.Password}");
            Console.WriteLine($"Hashed input password: {_passwordHasherService.HashPassword(password)}");*/
            // Validate password using PasswordHasherService
            var isPasswordValid = _passwordHasherService.VerifyPassword(user.Password, password);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid password.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            // Secret key for signing JWT - you should store this securely, not hardcoded
            var secretKey = _configuration["Authentication:Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT Secret Key is missing from configuration.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Set the claims for the JWT token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Assuming Role is an enum or string
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }

    // LoginRequest class for POST login method
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
