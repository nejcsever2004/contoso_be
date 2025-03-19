using System;
using System.ComponentModel.DataAnnotations;

namespace Contoso.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }  // Primary Key

        [Required]
        [StringLength(255)]
        public string? FullName { get; set; }  // Nullable string property for Full Name

        [Required]
        [StringLength(255)]
        public string? Email { get; set; }  // Nullable string property for Email

        [Required]
        [StringLength(255)]
        public string? Password { get; set; }

        [Display(Name = "Role")]
        [StringLength(255)]
        [Required]
        public string? Role { get; set; }  // Nullable string property for Role (Teacher, Student)

        public int? DepartmentID { get; set; }  // Nullable foreign key to Department

        public string? ProfileDocument { get; set; }  // Nullable string for Profile Document

        // Navigation property
        public Department? Department { get; set; }
    }
}
