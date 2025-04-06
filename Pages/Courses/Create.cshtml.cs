using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Pages.Courses
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
            //View data funkcionira kot funkcija za prikaz elementov na strani ob metodi get ko pridemo na stran
        ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "DepartmentName");
        ViewData["TeacherID"] = new SelectList(_context.Users, "UserID", "Email");
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
