
using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Services;
using CampusPlacementManagement.Repositories;
using CampusPlacementManagement.Enums;
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
        private readonly IPlacementOfficerRepository _placementOfficerRepository;
        private readonly IStudentRepository _studentRepository;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            IPlacementOfficerRepository placementOfficerRepository,
            IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _placementOfficerRepository = placementOfficerRepository;
            _studentRepository = studentRepository;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null ||
                !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Placement Officer approval check
            var officer = await _placementOfficerRepository.GetByUserIdAsync(user.Id);

            if (officer != null)
            {
                if (officer.Status != PlacementOfficerStatus.Approved)
                {
                    return Unauthorized(new
                    {
                        message = "Your placement officer account is not approved or active."
                    });
                }

                if (officer.College == null ||
                    officer.College.Status != CollegeStatus.Approved)
                {
                    return Unauthorized(new
                    {
                        message = "Your college is not currently approved."
                    });
                }
            }

            // Student approval check
            var student = await _studentRepository.GetByUserIdAsync(user.Id);

            if (student != null)
            {
                if (student.Status != StudentStatus.Approved)
                {
                    return Unauthorized(new
                    {
                        message = "Your student account is not approved or active."
                    });
                }

                if (student.College == null ||
                    student.College.Status != CollegeStatus.Approved)
                {
                    return Unauthorized(new
                    {
                        message = "Your college is not currently approved."
                    });
                }
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
