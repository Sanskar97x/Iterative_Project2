using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs
{
        public class CreateJobDto
        {
            // Basic Information
            public string JobTitle { get; set; }
            public string JobDescription { get; set; }
            public string JobPosition { get; set; }
        [Required] public JobType JobType { get; set; }

        // Job Details
        public string JobQualification { get; set; }
            public string JobResponsibilities { get; set; }
            public string JobRequirements { get; set; }
        [Range(0, 60)] public int JobExperience { get; set; }
        [Range(1, 10000)] public int? NumberOfVacancies { get; set; }

        // Salary Information
        [Range(0, int.MaxValue)] public int JobMinSalary { get; set; }
        [Range(0, int.MaxValue)] public int JobMaxSalary { get; set; }
        [Required, StringLength(3, MinimumLength = 3)] public string SalaryCurrency { get; set; } = "NPR";

        [Required, MaxLength(150)] public string JobLocation { get; set; } = string.Empty;
        public bool IsRemote { get; set; }

        public DateTime JobExpiredAt { get; set; }
        public DateTime ApplicationDeadline { get; set; }

        [Required] public Guid CompanyId { get; set; }
    }
    
}
