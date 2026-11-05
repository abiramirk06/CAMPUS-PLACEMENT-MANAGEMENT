
using CampusPlacementManagement.DTOs.College;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/colleges")]
    public class CollegesController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegesController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        // 1. Public college registration
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCollegeRequest request)
        {
            try
            {
                var result = await _collegeService.RegisterAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.CollegeId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // 2. Get all colleges
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var colleges = await _collegeService.GetAllAsync();

            return Ok(colleges);
        }

        // 3. Get pending colleges
        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPending()
        {
            var colleges = await _collegeService.GetPendingAsync();

            return Ok(colleges);
        }

        // 4. Get college by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            var college = await _collegeService.GetByIdAsync(id);

            if (college == null)
            {
                return NotFound(new
                {
                    message = "College not found."
                });
            }

            return Ok(college);
        }

        // 5. Update college
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCollegeRequest request)
        {
            try
            {
                var college = await _collegeService.UpdateAsync(id, request);

                if (college == null)
                {
                    return NotFound(new
                    {
                        message = "College not found."
                    });
                }

                return Ok(college);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // 6. Approve college
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public Task<IActionResult> Approve(int id)
        {
            return ChangeCollegeStatus(
                id,
                CollegeStatus.Approved,
                "College approved successfully.");
        }

        // 7. Reject college
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public Task<IActionResult> Reject(int id)
        {
            return ChangeCollegeStatus(
                id,
                CollegeStatus.Rejected,
                "College rejected successfully.");
        }

        // 8. Suspend college
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public Task<IActionResult> Suspend(int id)
        {
            return ChangeCollegeStatus(
                id,
                CollegeStatus.Suspended,
                "College suspended successfully.");
        }

        // Common status change handler
        private async Task<IActionResult> ChangeCollegeStatus(
            int id,
            CollegeStatus status,
            string successMessage)
        {
            try
            {
                var success = await _collegeService.ChangeStatusAsync(
                    id, status);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "College not found."
                    });
                }

                return Ok(new
                {
                    message = successMessage
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
