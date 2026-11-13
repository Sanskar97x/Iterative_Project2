using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.JobDto
{
    public class JobResponseDto : JobListDto
    {
        public string JobDescription { get; set; } = string.Empty;
        public string JobQualification { get; set; }
        public string JobResponsibilities { get; set; }
        public string JobRequirements { get; set; }
        public int NumberOfVacancies { get; set; }
        public DateTime JobExpiredAt { get; set; }
        public bool IsActive { get; set; }
    }
}
