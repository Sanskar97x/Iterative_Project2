using Iterative_Project2.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{
    public class JobApplication
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public Guid JobId { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public DateTime? StatusUpdatedAt { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        [MaxLength(4000)] public string CoverLetter { get; set; }
        [MaxLength(500)] public string ResumeUrl { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public Job Job { get; set; } = null!;
    }
}