using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.JobDto
{
    public class JobFilterRequestDto
    {
        public string Keyword { get; set; }
        public string Location { get; set; }
        public bool IsRemote { get; set; }
        public JobType JobType { get; set; }
        public JobPosition JobPosition { get; set; }
        public int? MinSalary { get; set; }
        public int? MaxExperience { get; set; }
        public Guid? CompanyId { get; set; }

        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 50)] public int PageSize { get; set; } = 10;
    }
}
