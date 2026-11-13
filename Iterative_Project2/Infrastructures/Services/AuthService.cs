using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Enums;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        public AuthService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        public async Task<Tuple<int, string>> Register(RegisterDto registerDto)
        {

            try
            {
                // AnyAsync() checks asynchronously whether any record matches the specified condition and returns true or false
                var existingUser = await _context.Users.AnyAsync(x => x.Email == registerDto.Email);

                if (existingUser)
                {
                    return new Tuple<int, string>(0, "This User is already exist, please register");
                }


                var user = new User()
                {
                    Fullname = registerDto.Fullname,
                    Email = registerDto.Username,
                  // Password = registerDto.Password,
                    Username = registerDto.Username,
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Creates a new Tuple object with an integer and a string, then return it.
                return new Tuple<int, string>(1, "Registered Successfully");
            }
            catch
            {
                throw;
            }
        }

        public async Task<Tuple<int, LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Identifier || x.Username == loginDto.Identifier);

                if (existingUser == null || loginDto.Password != existingUser.PasswordHash)
                {
                    return new Tuple<int, LoginResponseDto>(0, new LoginResponseDto
                    {
                        Message = "Invalid Email or Password",
                        Token = null
                    });
                }

                var token = _jwtService.GenerateToken(existingUser.Id, existingUser.Email);

                return new Tuple<int, LoginResponseDto>(1, new LoginResponseDto
                {
                    Message = "Login Successful",
                    Token = token
                });

            }
            catch
            {
                throw;
            }

        }
    }
}
