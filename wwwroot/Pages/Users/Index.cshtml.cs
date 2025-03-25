using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Text;

namespace Contoso.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly Contoso.Data.SchoolContext _context;

        public IndexModel(Contoso.Data.SchoolContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;

        public async Task OnGetAsync()
        {
            User = await _context.Users
                .Include(u => u.Department).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var users = await _context.Users.Include(u => u.Department).ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("UserID,FullName,Email,Role,Department");

            foreach(var user in users)
            {
                csvBuilder.AppendLine($"{user.UserID},{user.FullName},{user.Email},{user.Role},{user.Department?.DepartmentName}");
            }

            byte[] fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(fileBytes, "text/csv", "Users.csv");
        }
    }
}
