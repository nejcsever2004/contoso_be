using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Models;
using Contoso.Data;
using Microsoft.AspNetCore.Http;

namespace Contoso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesAndScheduleController : ControllerBase
    {
        private readonly SchoolContext _context;

        public GradesAndScheduleController(SchoolContext context)
        {
            _context = context;
        }

        // GET https://localhost:7062/api/GradesAndSchedule
        [HttpGet]
        public async Task<IActionResult> GetGradesAndSchedule()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                Console.WriteLine("UserId is not found in session.");
                return Unauthorized("User not logged in.");
            }

            var currentUser = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);

            if (currentUser == null || currentUser.Role != "Student")
            {
                return Forbid("Access denied.");
            }

            var enrolledCourses = await _context.Courses
                .Where(c => _context.Grades.Any(g => g.StudentID == userId.Value && g.CourseID == c.CourseID))
                .Include(c => c.Teacher)
                .ToListAsync();

            var grades = await _context.Grades
                .Where(g => g.StudentID == userId.Value)
                .ToDictionaryAsync(g => g.CourseID, g => g.GradeValue);

            var schedule = GenerateSchedule(enrolledCourses);

            return Ok(new { CurrentUser = currentUser, EnrolledCourses = enrolledCourses, Grades = grades, Schedule = schedule });
        }

        private List<ScheduleEntry> GenerateSchedule(List<Course> courses)
        {
            var schedule = new List<ScheduleEntry>();
            DateTime morningStart = DateTime.Today.AddHours(8).AddMinutes(30);
            DateTime afternoonStart = DateTime.Today.AddHours(16);

            int morningCount = 0, afternoonCount = 0;
            foreach (var course in courses)
            {
                DateTime classTime = morningCount < 2
                    ? morningStart.AddMinutes(morningCount++ * 120)
                    : afternoonStart.AddMinutes(afternoonCount++ * 120);

                schedule.Add(new ScheduleEntry
                {
                    Course = course,
                    StartTime = classTime,
                    EndTime = classTime.AddMinutes(120)
                });
            }
            return schedule;
        }
    }

    public class ScheduleEntry
    {
        public Course? Course { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
