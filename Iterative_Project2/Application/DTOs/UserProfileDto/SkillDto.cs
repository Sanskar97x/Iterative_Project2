using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.UserProfileDto
{
    public class SkillDto
    {
        public Guid? Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [Required] public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
