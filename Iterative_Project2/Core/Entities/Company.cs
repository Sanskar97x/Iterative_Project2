using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Core.Entities
{
    public class Company
    {
        // Primary Key
        public Guid Id { get; set; }
        public Guid OwnerUserId { get; set; }

        // Foreign Key to User
        [Required, MaxLength(150)] 
        public string CompanyName { get; set; } = string.Empty;
        [MaxLength(2000)] 
        public string Description { get; set; }
        [MaxLength(255)] 
        public string Website { get; set; }
        [MaxLength(150)] 
        public string Location { get; set; }

        // Navigation Property
        public User OwnerUser { get; set; } = null!;

        // One-to-Many Relationship with Jobs
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        [MaxLength(100)] public string Industry { get; set; }
        public CompanySize? CompanySize { get; set; }
        public int? FoundedYear { get; set; }
        [MaxLength(150)] public string ContactEmail { get; set; }
        [MaxLength(30)] public string ContactPhone { get; set; }

        public bool IsVerified { get; set; } = false;                  // set by an Admin
        public bool IsActive { get; set; } = true;                     // employer can deactivate
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}