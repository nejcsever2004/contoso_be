using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contoso.Helpers;
using Contoso.Data;
using Contoso.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Contoso.Pages.GradesAndSchedule
{
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;

        private readonly TimeSpan startTime = new TimeSpan(8, 30, 0);
        private readonly TimeSpan endTime = new TimeSpan(12, 40, 0);
        private readonly TimeSpan onlineStartTime = new TimeSpan(16, 30, 0);
        private readonly TimeSpan onlineEndTime = new TimeSpan(20, 0, 0);

        private readonly int periodLength = 45; // minutes

        public IndexModel(SchoolContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            int? userID = HttpContext.Session.GetInt32("userId");
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userID == null || userRole != "Student")
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userID);

            if (user == null)
            {
                return NotFound();
            }

            var grades = await _context.Grades
                .Where(g => g.StudentID == userID)
                .Include(g => g.Course)
                .ToListAsync();

            Data = new IndexPageModel
            {
                Username = User.Identity.Name,
                TimeTable = GenerateTimeTable(),
                Grades = grades
            };

            return Page();
        }

        public List<TimetableEntry> GenerateTimeTable()
        {
            List<TimetableEntry> timetable = new List<TimetableEntry>();
            DateTime currentTime = DateTime.Today.Add(startTime);

            while (currentTime.TimeOfDay < endTime)
            {
                timetable.Add(new TimetableEntry
                {
                    StartTime = currentTime,
                    EndTime = currentTime.AddMinutes(periodLength)
                });
                currentTime = currentTime.AddMinutes(periodLength);
            }

            return timetable;
        }

        public class TimetableEntry
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        // Changed name from IndexModel to IndexPageModel to avoid conflict
        public class IndexPageModel
        {
            public List<TimetableEntry>? TimeTable { get; set; }
            public List<Grade>? Grades { get; set; }
            public string? Username { get; internal set; }
        }

        public IndexPageModel Data { get; set; }  // This is to store the data we pass to the page.
    }
}
