using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Contoso.Data;
using Contoso.Models;
using Contoso.Helpers;
using Microsoft.AspNetCore.Http;

namespace Contoso.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly SchoolContext _context;  // Database connection
        private readonly PasswordHasherService _passwordHasherService;

        // Constructor to inject services
        public RegisterModel(SchoolContext context, PasswordHasherService passwordHasherService)
        {
            _context = context;
            _passwordHasherService = passwordHasherService;  // Initialize the password hasher
        }

        [BindProperty]
        public User User { get; set; }

        [BindProperty]
        public string Password { get; set; }  // For password input

        [BindProperty]
        public string ConfirmPassword { get; set; }  // For confirm password input

        [BindProperty]
        public IFormFile FileUpload { get; set; }

        // OnGet method to initialize ViewData
        public void OnGet()
        {
            ViewData["Roles"] = new List<string> { "Student", "Teacher" }; // Initialize ViewData here
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the User object is null
            if (User == null)
            {
                ModelState.AddModelError(string.Empty, "User information is missing.");
                return Page();
            }

            // Validate password confirmation
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords don't match!");
                return Page();
            }

            // Check if the email already exists in the database
            if (_context.Users.Any(u => u.Email == User.Email))
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                return Page();
            }

            // Hash the password using PasswordHasherService
            var hashedPassword = _passwordHasherService.HashPassword(Password);

            string profileDocumentPath = null;
            if(FileUpload != null && FileUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFilename = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFilename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }

                profileDocumentPath = $"/uploads/{uniqueFilename}";
            }

            // Add the user to the database with hashed password
            var newUser = new User
            {
                FullName = User.FullName,
                Email = User.Email,
                Password = hashedPassword,  // Store the hashed password
                Role = User.Role,
                DepartmentID = User.DepartmentID,
                ProfileDocument = User.ProfileDocument
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Redirect to login page after successful registration
            return RedirectToPage("/Login");
        }
    }
}
