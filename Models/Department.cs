using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Contoso.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(255)]
        public string? DepartmentName { get; set; }
        [JsonIgnore]
        public ICollection<User>? Users { get; set; }
        [JsonIgnore]
        public ICollection<Course>? Courses { get; set; }
    }
}
