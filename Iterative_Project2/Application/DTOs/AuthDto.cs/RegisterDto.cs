using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Fullname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(225)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
