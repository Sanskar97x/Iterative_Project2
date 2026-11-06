using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Application.DTOs.AuthDto.cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public interface IAuthService
    {
        Task<Tuple<int, LoginResponseDto>> Login(LoginDto loginDto);
        Task<Tuple<int, string>> Register(RegisterDto registerDto);
    }
}
