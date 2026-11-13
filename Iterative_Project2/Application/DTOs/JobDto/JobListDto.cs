using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.JobDto
{
    public class JobListDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public JobPosition JobPosition { get; set; }
        public JobType JobType { get; set; }
        public int JobExperience { get; set; }
        public int JobMinSalary { get; set; }
        public int JobMaxSalary { get; set; }
        public string SalaryCurrency { get; set; } = string.Empty;
        public string JobLocation { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public DateTime JobCreatedAt { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
