using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Application.DTOs.UserProfileDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public interface IUserService
    {
        Task<UserProfileResponseDto> GetUserProfile();
        //Task<UpdateUserProfileResponseDto> UpdateUserProfile(UpdateUserProfileDto updateUserProfileDto);
    }
}
