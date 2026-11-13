using Iterative_Project2.Application.DTOs.UserProfileDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs
{
    public class UpdateUserProfileDto
    {
        [Required, MaxLength(100)] public string Fullname { get; set; } = string.Empty;
        [MaxLength(20)] public string PhoneNumber { get; set; }
        [MaxLength(255)] public string Address { get; set; }
        [MaxLength(2000)] public string Bio { get; set; }
        [MaxLength(500)] public string ResumeUrl { get; set; }

        // Skills
        public List<string> Skills { get; set; }
        // Education
        public List<EducationDto> Educations { get; set; }
        // Experience
        public List<ExperienceDto> Experiences { get; set; }
    }
}
