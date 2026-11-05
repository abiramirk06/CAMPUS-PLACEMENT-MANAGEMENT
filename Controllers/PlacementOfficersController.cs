using System.Security.Claims;
using CampusPlacementManagement.DTOs.PlacementOfficer;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/placement-officers")]
    public class PlacementOfficersController : ControllerBase
    {
        private readonly IPlacementOfficerService _service;

        public PlacementOfficersController(IPlacementOfficerService service)
        {
            _service = service;
        }

        // Public registration
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterPlacementOfficerRequest request)
        {
            try
            {
                var result = await _service.RegisterAsync(request);

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // Get all officers - SuperAdmin only
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // Get pending officers - SuperAdmin only
        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await _service.GetPendingAsync());
        }

        // Get officer by ID - SuperAdmin only
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            var officer = await _service.GetByIdAsync(id);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement officer not found."
                });
            }

            return Ok(officer);
        }

        // Approve officer
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            string? approvedBy = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(approvedBy))
            {
                return Unauthorized();
            }

            return await ChangeStatus(
                () => _service.ApproveAsync(id, approvedBy),
                "Placement officer approved successfully.");
        }

        // Reject officer
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            return await ChangeStatus(
                () => _service.RejectAsync(id),
                "Placement officer rejected successfully.");
        }

        // Suspend officer
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Suspend(int id)
        {
            return await ChangeStatus(
                () => _service.SuspendAsync(id),
                "Placement officer suspended successfully.");
        }

        // Get logged-in officer profile
        [HttpGet("me")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> GetMyProfile()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var officer = await _service.GetMyProfileAsync(userId);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement officer profile not found."
                });
            }

            return Ok(officer);
        }

        private async Task<IActionResult> ChangeStatus(
            Func<Task<bool>> action,
            string successMessage)
        {
            try
            {
                bool success = await action();

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "Placement officer not found."
                    });
                }

                return Ok(new { message = successMessage });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}