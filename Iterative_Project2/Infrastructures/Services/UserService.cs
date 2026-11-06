using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;


        public UserService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public async Task<UserProfileResponseDto> GetUserProfile()
        {
            var userID = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userID);

            return new UserProfileResponseDto
            {
                Fullname = user.Fullname,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Bio = user.Bio,
            };
        }

        /*public async Task<UpdateUserProfileResponseDto> UpdateUserProfile(UpdateUserProfileDto updateUserProfileDto)
        {
        }*/
        private Guid GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            return Guid.Parse(userId);
        }
    
    }
}
