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
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("UserId", user.UserID.ToString()),  // Add UserID as a custom claim
        new Claim(ClaimTypes.Role, user.Role)
    };

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
