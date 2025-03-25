using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Pages.Grades
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
        ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title");
        ViewData["StudentID"] = new SelectList(_context.Users, "UserID", "Email");
            return Page();
        }

        [BindProperty]
        public Grade Grade { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Grades.Add(Grade);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
