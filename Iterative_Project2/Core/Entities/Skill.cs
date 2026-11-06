using System;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{
    public class Skill
    {
        // Primary Key
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string ProficiencyLevel { get; set; }

        // Foreign Key
        public Guid CandidateId { get; set; }

        // Navigation Property
        public Candidate Candidate { get; set; } = null!;
    }
}