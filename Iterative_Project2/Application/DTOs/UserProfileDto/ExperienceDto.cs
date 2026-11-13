using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.UserProfileDto
{
    public class ExperienceDto
    {
        public Guid? Id { get; set; }
        [Required, MaxLength(150)] public string JobTitle { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string CompanyName { get; set; } = string.Empty;
        [Required] public JobType EmploymentType { get; set; }
        [MaxLength(150)] public string Location { get; set; }
        [MaxLength(2000)] public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentlyWorking { get; set; }
    }
}
