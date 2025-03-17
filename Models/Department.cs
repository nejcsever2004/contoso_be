using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Contoso.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(255)]
        public string? DepartmentName { get; set; }

        public ICollection<User>? Users { get; set; }
        public ICollection<Course>? Courses { get; set; }
    }
}
