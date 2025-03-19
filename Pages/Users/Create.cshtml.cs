using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly Contoso.Data.SchoolContext _context;

        public CreateModel(Contoso.Data.SchoolContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            User = new User();
            ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "DepartmentName");
            return Page();
        }

        [BindProperty]
        public User? User { get; set; } = default!;

        [BindProperty]
        public IFormFile? FileUpload { get; set; } // Bind file input

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "DepartmentName");
                return Page();
            }

            // Add User to database
            _context.Users.Add(User);
            await _context.SaveChangesAsync(); // Save to generate UserID

            // Handle file upload if provided
            if (FileUpload != null && FileUpload.Length > 0)
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

                // Save relative path to User profile
                User.ProfileDocument = $"/uploads/{uniqueFilename}";
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
