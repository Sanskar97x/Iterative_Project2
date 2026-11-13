using Iterative_Project2.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{
    public class Experience
    {
        // Primary Key
        public Guid Id { get; set; }

        [Required, MaxLength(150)]
        public string JobTitle { get; set; }

        [Required, MaxLength(150)]
        public string CompanyName { get; set; }

        public JobType EmploymentType { get; set; }

        [MaxLength(150)] 
        public string Location { get; set; }
        [MaxLength(2000)] 
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentlyWorking { get; set; }

        // Foreign Key
        public Guid CandidateId { get; set; }

        // Navigation Property
        public Candidate Candidate { get; set; } = null!;
    }
}