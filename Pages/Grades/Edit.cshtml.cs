using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Pages.Grades
{
    public class EditModel : PageModel
    {
        private readonly Contoso.Data.SchoolContext _context;

        public EditModel(Contoso.Data.SchoolContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Grade Grade { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade =  await _context.Grades.FirstOrDefaultAsync(m => m.GradeID == id);
            if (grade == null)
            {
                return NotFound();
            }
            Grade = grade;
           ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title");
           ViewData["StudentID"] = new SelectList(_context.Users, "UserID", "Email");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Grade).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(Grade.GradeID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.GradeID == id);
        }
    }
}
