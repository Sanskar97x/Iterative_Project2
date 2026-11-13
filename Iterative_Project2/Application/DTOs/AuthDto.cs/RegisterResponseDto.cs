using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs.AuthDto.cs
{
    public class RegisterResponseDto
    {
        public Guid UserId { get; set;}
        public string Message { get; set; } = string.Empty;
    }
}
