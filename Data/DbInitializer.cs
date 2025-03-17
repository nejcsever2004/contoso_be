using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using Contoso.Data;
using Contoso.Models;

namespace Contoso.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SchoolContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var courses = new Course[]
            {
            };

            context.Courses.AddRange(courses);
            context.SaveChanges();

            var Department = new Department[]
            {
            };

            context.Departments.AddRange(Department);
            context.SaveChanges();
        }
    }
}