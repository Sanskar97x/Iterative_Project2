using System;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{
    public class Education
    {
        // Primary Key
        public Guid Id { get; set; }

        [Required, MaxLength(150)] 
        public string Institution { get; set; } = string.Empty;
        [Required, MaxLength(100)] 
        public string Degree { get; set; } = string.Empty;
        [MaxLength(100)] 
        public string FieldOfStudy { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentlyStudying { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        // Foreign Key
        public Guid CandidateId { get; set; }

        // Navigation Property
        public Candidate Candidate { get; set; } = null!;
    }
}