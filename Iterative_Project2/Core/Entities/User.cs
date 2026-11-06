using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Iterative_Project2.Core.Enums;

namespace Iterative_Project2.Core.Entities
{
    public class User
    {
        // Primary Key
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Fullname { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(225)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        [MaxLength(255)]
        public string Address { get; set; }
        
        [MaxLength(2000)]
        public string Bio { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int YearsOfExperience { get; set; } = 0;
        
        // One-to-One Relationship with Candidate
        public Candidate Candidate { get; set; }
        
        // One-to-Many Relationship with Company
        public ICollection<Company> OwnedCompanies { get; set; } = new List<Company>();
    }
}