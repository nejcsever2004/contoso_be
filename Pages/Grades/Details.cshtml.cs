using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Pages.Grades
{
    public class DetailsModel : PageModel
    {
        private readonly Contoso.Data.SchoolContext _context;

        public DetailsModel(Contoso.Data.SchoolContext context)
        {
            _context = context;
        }

        public Grade Grade { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades.FirstOrDefaultAsync(m => m.GradeID == id);
            if (grade == null)
            {
                return NotFound();
            }
            else
            {
                Grade = grade;
            }
            return Page();
        }
    }
}
