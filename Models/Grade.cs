using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contoso.Models
{
    public class Grade
    {
        [Key]
        public int GradeID { get; set; }  // Primary Key

        public int StudentID { get; set; }  // Foreign key to User (Student)

        public int CourseID { get; set; }  // Foreign key to Course

        [Range(0.00, 100.00)]
        public decimal GradeValue { get; set; }  // Grade between 0 and 100

        // Navigation properties
        public User? Student { get; set; }  // Navigation property to Student (User)
        public Course? Course { get; set; }  // Navigation property to Course
    }
}
