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
    }
}