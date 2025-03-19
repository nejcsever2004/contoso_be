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
    public class LoginModel : PageModel
    {
        private readonly SchoolContext _context;  // Database connection
        private readonly PasswordHasherService _passwordHasherService;

        public LoginModel(SchoolContext context, PasswordHasherService passwordHasherService)
        {
            _context = context;
            _passwordHasherService = passwordHasherService;  // Initialize the password hasher
        }

        [BindProperty]
        public string Email { get; set; }  // For email input

        [BindProperty]
        public string Password { get; set; }  // For password input

        public string ErrorMessage { get; set; }  // For showing error messages

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate email and password
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            // Retrieve the user by email from the database
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Verify the provided password with the stored hashed password
            var isPasswordValid = _passwordHasherService.VerifyPassword(user.Password, Password);

            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Store the user ID and role in the session
            HttpContext.Session.SetInt32("UserId", user.UserID);  // Store the UserID in session
            HttpContext.Session.SetString("UserRole", user.Role);  // Store the User's role

            // Debugging: log session values for troubleshooting
            Console.WriteLine($"User logged in: {user.UserID}, Role: {user.Role}");

            // Redirect based on the user's role
            if (user.Role == "Student")
            {
                // Redirect to Grades and Schedule page for Students
                return RedirectToPage("/Users/GradesAndSchedule");
            }
            else if (user.Role == "Teacher")
            {
                // Redirect to Users page for Teachers
                return RedirectToPage("/Users/Index");
            }
            else
            {
                // Redirect to Home page if role is neither Student nor Teacher
                return RedirectToPage("/Index");
            }
        }
    }
}
