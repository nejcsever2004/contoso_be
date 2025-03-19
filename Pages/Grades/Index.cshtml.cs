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
    public class IndexModel : PageModel
    {
        private readonly Contoso.Data.SchoolContext _context;

        public IndexModel(Contoso.Data.SchoolContext context)
        {
            _context = context;
        }

        public IList<Grade> Grade { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Grade = await _context.Grades
                .Include(g => g.Course)
                .Include(g => g.Student).ToListAsync();
        }
    }
}
