using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contoso.Models
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }
        [Required]
        [StringLength(255)]
        public string? Title { get; set; }
        public int? TeacherID { get; set; }
        public int? DepartmentID { get; set; }

        public User? Teacher { get; set; }

        //public User? Student { get; set; }
        public Department? Department { get; set; }
    }
}
