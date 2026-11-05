using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email.Trim());

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            bool passwordValid = await _userManager.CheckPasswordAsync(
                user, request.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            string token = _jwtService.GenerateToken(user, roles);

            return Ok(new LoginResponse
            {
                Message = "Login successful.",
                Token = token,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Roles = roles
            });
        }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = [];
    }
}