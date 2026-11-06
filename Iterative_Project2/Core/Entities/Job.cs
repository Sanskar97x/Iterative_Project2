using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{

        public class Job
        {
            public Guid JobId { get; set; }

            [Required, MaxLength(150)] public string JobTitle { get; set; } = string.Empty;
            [Required] public string JobDescription { get; set; } = string.Empty;
            public JobPosition JobPosition { get; set; }
            public JobType JobType { get; set; }

            public string JobQualification { get; set; }
            public string JobResponsibilities { get; set; }
            public string JobRequirements { get; set; }
            public int JobExperience { get; set; }               // years
            public int? NumberOfVacancies { get; set; }

            public int JobMinSalary { get; set; }
            public int JobMaxSalary { get; set; }
            [MaxLength(3)] public string SalaryCurrency { get; set; } = "NPR";

            [MaxLength(150)] public string JobLocation { get; set; } = string.Empty;
            public bool IsRemote { get; set; }

            public DateTime JobCreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime JobExpiredAt { get; set; }
            public DateTime ApplicationDeadline { get; set; }
            public bool IsActive { get; set; } = true;

            public Guid CompanyId { get; set; }
            public Company Company { get; set; } = null!;
            public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        }
    }
