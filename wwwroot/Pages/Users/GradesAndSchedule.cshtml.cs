using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Contoso.Models;
using Contoso.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contoso.Users
{
    public class GradesAndScheduleModel : PageModel
    {
        private readonly SchoolContext _context;

        public GradesAndScheduleModel(SchoolContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; set; }
        public List<Course> EnrolledCourses { get; set; } = new List<Course>();
        public Dictionary<int, decimal> Grades { get; set; } = new Dictionary<int, decimal>();
        public List<ScheduleEntry> Schedule { get; set; } = new List<ScheduleEntry>();

        public async Task<IActionResult> OnGetAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                Console.WriteLine("UserID is not found in session.");
                return RedirectToPage("/Login");
            }

            CurrentUser = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);

            if (CurrentUser == null || CurrentUser.Role != "Student")
            {
                Console.WriteLine("User not found or not a student");
                return RedirectToPage("/AccessDenied");
            }

            EnrolledCourses = await _context.Courses
                .Where(c => _context.Grades.Any(g => g.StudentID == userId.Value && g.CourseID == c.CourseID))
                .Include(c => c.Teacher)
                .ToListAsync();

            Grades = await _context.Grades
                .Where(g => g.StudentID == userId.Value)
                .ToDictionaryAsync(g => g.CourseID, g => g.GradeValue);

            GenerateSchedule();
            return Page();
        }

        private void GenerateSchedule()
        {
            Schedule.Clear();
            DateTime morningStart = DateTime.Today.AddHours(8).AddMinutes(30);
            DateTime afternoonStart = DateTime.Today.AddHours(16);

            int morningCount = 0, afternoonCount = 0;
            foreach (var course in EnrolledCourses)
            {
                DateTime classTime;
                if (morningCount < 2)
                {
                    classTime = morningStart.AddMinutes(morningCount * 120);
                    morningCount++;
                }
                else
                {
                    classTime = afternoonStart.AddMinutes(afternoonCount * 120);
                    afternoonCount++;
                }

                Schedule.Add(new ScheduleEntry
                {
                    Course = course,
                    StartTime = classTime,
                    EndTime = classTime.AddMinutes(120)
                });

                Console.WriteLine($"Scheduled {course.Title} from {classTime} to {classTime.AddMinutes(120)}");
            }
        }
    }

    public class ScheduleEntry
    {
        public Course? Course { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
