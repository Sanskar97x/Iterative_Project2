using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Generic_Collections;
using Iterative_Project2.Infrastructures.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.APIs.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult>Register([FromBody]RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.Register(registerDto);

                if (result.Item1 == 0)
                {
                    // T is replaced by string for the Data type, while result.Item2 provides the error message.
                    return BadRequest(ResponseResult<string>.Failure(null, result.Item2));
                }
                return Ok(new
                {
                    // data and message are properties created by new { }. They don't need to be declared
                    //data = (string)null,
                    message = result.Item2
                });
                //return Ok(result.Item2);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.Login(loginDto);

                if (result.Item1 == 0)
                {
                    
                    return BadRequest(ResponseResult<LoginResponseDto>.Failure(result.Item2, "Login Failed"));
                }
                return Ok(ResponseResult<LoginResponseDto>.Success(result.Item2,"Login Successful"));
            }
            catch
            {
                throw;
            }
        }
    }
}
