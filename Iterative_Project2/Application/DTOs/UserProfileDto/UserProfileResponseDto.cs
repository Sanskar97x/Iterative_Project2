using Iterative_Project2.Application.DTOs.UserProfileDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs
{
    public class UserProfileResponseDto
    {

        public Guid UserId { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Bio { get; set; }
        public string ResumeUrl { get; set; }
        public List<SkillDto> Skills { get; set; } = new();
        public List<EducationDto> Educations { get; set; } = new();
        public List<ExperienceDto> Experiences { get; set; } = new();
    }
}
