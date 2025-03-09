using Domain.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = Application.DTOs.RegisterRequest;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.FullName
            );

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(
                request.Email,
                request.Password
            );

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}



//{
//    "email": "rayhan@gmail.com",
//  "password": "1234",
//  "fullName": "Md Abu Rayhan"
//}
