using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Contoso.Data;
using Contoso.Models;
using Contoso.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Contoso.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SchoolContext _context;  // Database connection
        private readonly PasswordHasherService _passwordHasherService;
        private readonly IConfiguration _configuration;  // To access appsettings.json

        public LoginModel(SchoolContext context, PasswordHasherService passwordHasherService, IConfiguration configuration)
        {
            _context = context;
            _passwordHasherService = passwordHasherService;
            _configuration = configuration;
        }

        [BindProperty]
        public string Email { get; set; }  // For email input

        [BindProperty]
        public string Password { get; set; }  // For password input

        public string ErrorMessage { get; set; }  // For showing error messages

        public async Task<IActionResult> OnPostAsync(bool isJwtRequested = false)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null || !_passwordHasherService.VerifyPassword(user.Password, Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),  // User ID as claim
                new Claim(ClaimTypes.Name, user.FullName),  // User full name
                new Claim(ClaimTypes.Email, user.Email),  // User email
                new Claim(ClaimTypes.Role, user.Role),    // User role (Student, etc.)
            };

            // If JWT is requested, generate the JWT token
            if (isJwtRequested)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),  // Token expiration time
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.WriteToken(token);

                // Return the JWT token as a response
                return new JsonResult(new { token = jwtToken });
            }
            else
            {
                // Cookie authentication (old logic)
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // Set UserId in session
                HttpContext.Session.SetInt32("UserId", user.UserID); // Store UserID in session

                Console.WriteLine($"User logged in: {user.UserID}, Role: {user.Role}");

                return RedirectToPage(user.Role == "Student" ? "/Users/GradesAndSchedule" : "/Users/Index");
            }
        }
    }
}
