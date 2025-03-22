using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contoso.Data;
using Contoso.Helpers;
using Contoso.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Don't forget to include this for Include() to work.

namespace Contoso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegisterController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly PasswordHasherService _passwordHasherService;

        public UserRegisterController(SchoolContext context, PasswordHasherService passwordHasherService)
        {
            _context = context;
            _passwordHasherService = passwordHasherService;
        }

        // POST: api/UserRegisterController/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegistrationRequest request)
        {
            // Validation of user input
            if (request == null || string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("User information is missing.");
            }

            // Validate password confirmation
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            // Check if the email already exists in the database
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Email already exists.");
            }

            // Hash the password
            var hashedPassword = _passwordHasherService.HashPassword(request.Password);

            // Handle file upload
            string profileDocumentPath = null;
            if (request.FileUpload != null && request.FileUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFilename = $"{Guid.NewGuid()}_{Path.GetFileName(request.FileUpload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFilename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.FileUpload.CopyToAsync(stream);
                }

                profileDocumentPath = $"/uploads/{uniqueFilename}";
            }

            // Create a new user object
            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = hashedPassword,  // Store the hashed password
                Role = request.Role ?? "Student",  // Default to "Student" if no role is specified
                DepartmentID = request.DepartmentID,
                ProfileDocument = profileDocumentPath
            };

            // Add the user to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        // GET: api/UserRegisterController/get/{id}
        [HttpGet("get/{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        // GET: api/UserRegisterController/getAll
        [HttpGet("getAll")]
        public IActionResult GetAllUsers()
        {
            // Include Department details
            var users = _context.Users
                .Include(u => u.Department)  // Ensure this is included to load department info
                .ToList();

            if (!users.Any())
            {
                return NotFound("No users found.");
            }

            // Return a structured JSON response with relevant user information
            var result = users.Select(user => new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                DepartmentName = user.Department != null ? user.Department.DepartmentName : "No Department",  // Check if Department is null
                user.ProfileDocument
            }).ToList();

            // Return the result as JSON
            return Ok(result);
        }
    }

    // Request model for user registration
    public class UserRegistrationRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Role { get; set; } = "Student";  // Default role is "Student"
        public int DepartmentID { get; set; }
        public IFormFile? FileUpload { get; set; }

        public Department? Department { get; set; }  // Navigation to Department

    }
}
