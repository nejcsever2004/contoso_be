using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Models;
using Contoso.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

        // GET https://localhost:7062/api/GradesAndSchedule/gradesandschedule
        [HttpGet("gradesandschedule")]
        public async Task<IActionResult> GetGradesAndSchedule()
        {
            try
            {
                // Get user ID from the JWT token (this requires the user to be authenticated)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID claim is missing from token.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Invalid User ID format.");
                }

                // Fetch user data (with their department details)
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserID == userId);

                if (currentUser == null || currentUser.Role != "Student")
                {
                    // Returning a default user if the current user is missing or not a student
                    currentUser = new User
                    {
                        UserID = userId,
                        FullName = "Guest",
                        Email = "guest@example.com",
                        ProfileDocument = "default.jpg",
                        DepartmentID = 0 // Placeholder department ID
                    };
                }

                // Fetch enrolled courses with grades and teacher info
                var enrolledCourses = await _context.Courses
                    .Where(c => _context.Grades.Any(g => g.StudentID == userId && g.CourseID == c.CourseID))
                    .Include(c => c.Teacher)
                    .ToListAsync();

                // Fetch grades
                var grades = await _context.Grades
                    .Where(g => g.StudentID == userId)
                    .ToDictionaryAsync(g => g.CourseID, g => g.GradeValue);

                // Generate schedule
                var schedule = GenerateSchedule(enrolledCourses);

                // Return the data as a response
                return Ok(new
                {
                    CurrentUser = new
                    {
                        currentUser.UserID,
                        currentUser.FullName,
                        currentUser.Email,
                        ProfileDocument = currentUser.ProfileDocument ?? "default.jpg",
                        DepartmentID = currentUser.DepartmentID,
                        Role = currentUser.Role
                    },
                    EnrolledCourses = enrolledCourses.Select(c => new
                    {
                        c.CourseID,
                        c.Title,
                        TeacherName = c.Teacher.FullName
                    }),
                    Grades = enrolledCourses.Select(c => new
                    {
                        Course = c.Title,
                        Grade = grades.TryGetValue(c.CourseID, out decimal gradeValue) ? gradeValue.ToString("0.00") : "N/A"
                    }),
                    Schedule = schedule.Select(entry => new
                    {
                        Course = entry.Course.Title,
                        StartTime = entry.StartTime.ToString("MM/dd/yyyy h:mm tt"),
                        EndTime = entry.EndTime.ToString("MM/dd/yyyy h:mm tt")
                    })
                });
            }
            catch (Exception ex)
            {
                // Log the exception to the console for debugging
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST https://localhost:7062/api/GradesAndSchedule/gradesandschedule
        [HttpPost("gradesandschedule")]
        [Authorize(Roles = "Student", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostGradesAndSchedule([FromBody] User userDetails)
        {
            try
            {
                if (userDetails == null)
                {
                    return BadRequest("User details are required.");
                }

                // Get user ID from the JWT token (this requires the user to be authenticated)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID claim is missing from token.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Invalid User ID format.");
                }

                // Fetch user data (with their department details)
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserID == userId);

                if (currentUser == null || currentUser.Role != "Student")
                {
                    currentUser = new User
                    {
                        UserID = userId,
                        FullName = "Guest",
                        Email = "guest@example.com",
                        ProfileDocument = "default.jpg",
                        DepartmentID = 0 // Placeholder department ID
                    };
                }

                // Fetch enrolled courses with grades and teacher info
                var enrolledCourses = await _context.Courses
                    .Where(c => _context.Grades.Any(g => g.StudentID == userDetails.UserID && g.CourseID == c.CourseID))
                    .Include(c => c.Teacher)
                    .ToListAsync();

                // Fetch grades
                var grades = await _context.Grades
    .Where(g => g.StudentID == userDetails.UserID).ToDictionaryAsync(g => g.CourseID, g => g.GradeValue);
                // Generate schedule
                var schedule = GenerateSchedule(enrolledCourses);

                // Return the data as a response
                return Ok(new
                {
                    CurrentUser = new
                    {
                        currentUser.UserID,
                        currentUser.FullName,
                        currentUser.Email,
                        ProfileDocument = currentUser.ProfileDocument ?? "default.jpg",
                        DepartmentID = currentUser.DepartmentID,
                        Role = currentUser.Role
                    },
                    EnrolledCourses = enrolledCourses.Select(c => new
                    {
                        c.CourseID,
                        c.Title,
                        TeacherName = c.Teacher.FullName
                    }),
                    Grades = enrolledCourses.Select(c => new
                    {
                        Course = c.Title,
                        Grade = grades.TryGetValue(c.CourseID, out decimal gradeValue) ? gradeValue.ToString("0.00") : "N/A"
                    }),
                    Schedule = schedule.Select(entry => new
                    {
                        Course = entry.Course.Title,
                        StartTime = entry.StartTime.ToString("MM/dd/yyyy h:mm tt"),
                        EndTime = entry.EndTime.ToString("MM/dd/yyyy h:mm tt")
                    })
                });
            }
            catch (Exception ex)
            {
                // Log the exception to the console for debugging
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // Private method to generate a class schedule for the student
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
