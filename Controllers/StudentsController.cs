
using System.Security.Claims;
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private bool IsSuperAdmin =>
            User.IsInRole("SuperAdmin");

        [HttpPost("enroll")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> Enroll(
            [FromBody] EnrollStudentRequest request)
        {
            try
            {
                var result = await _service.EnrollAsync(
                    request, CurrentUserId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.StudentId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }

        [HttpPost("bulk-enroll")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> BulkEnroll(
            [FromBody] List<EnrollStudentRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest(new
                {
                    message = "At least one student is required."
                });

            if (requests.Count > 100)
                return BadRequest(new
                {
                    message = "A maximum of 100 students can be enrolled at once."
                });

            try
            {
                var result = await _service.BulkEnrollAsync(
                    requests, CurrentUserId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync(
                    CurrentUserId, IsSuperAdmin);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var result = await _service.GetPendingAsync(CurrentUserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _service.GetByIdAsync(id);

            if (student == null)
                return NotFound(new { message = "Student not found." });

            if (!IsSuperAdmin)
            {
                bool isOwnStudent = student.UserId == CurrentUserId;

                if (User.IsInRole("Student") && !isOwnStudent)
                    return Forbid();

                if (User.IsInRole("PlacementOfficer"))
                {
                    var allStudents = await _service.GetAllAsync(
                        CurrentUserId, false);

                    if (!allStudents.Any(s => s.StudentId == id))
                        return Forbid();
                }
            }

            return Ok(student);
        }

        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                bool result = await _service.ApproveAsync(id, CurrentUserId);

                if (!result)
                    return NotFound(new { message = "Student not found." });

                return Ok(new { message = "Student approved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                bool result = await _service.RejectAsync(id, CurrentUserId);

                if (!result)
                    return NotFound(new { message = "Student not found." });

                return Ok(new { message = "Student rejected successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                bool result = await _service.SuspendAsync(
                    id, CurrentUserId, IsSuperAdmin);

                if (!result)
                    return NotFound(new { message = "Student not found." });

                return Ok(new { message = "Student suspended successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateStudentRequest request)
        {
            try
            {
                bool result = await _service.UpdateAsync(
                    id, request, CurrentUserId, IsSuperAdmin);

                if (!result)
                    return NotFound(new { message = "Student not found." });

                return Ok(new { message = "Student updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMe()
        {
            var student = await _service.GetMyProfileAsync(CurrentUserId);

            if (student == null)
                return NotFound(new { message = "Student profile not found." });

            return Ok(student);
        }
    }
}
