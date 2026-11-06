using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Entities
{
    public class Candidate
    {
        public Guid Id { get; set; }
        // One-to-One relationship with User
        public User User { get; set; } = null;

        [MaxLength(500)] 
        public string ResumeUrl { get; set; }

        // One-to-Many Relationships
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public ICollection<JobApplication> Applications { get; set; }
    = new List<JobApplication>();
    }
}
