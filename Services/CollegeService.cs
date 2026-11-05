
using CampusPlacementManagement.DTOs.College;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories;

namespace CampusPlacementManagement.Services
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _repository;

        public CollegeService(ICollegeRepository repository)
        {
            _repository = repository;
        }

        // Get all colleges
        public async Task<IEnumerable<CollegeResponse>> GetAllAsync()
        {
            var colleges = await _repository.GetAllAsync();

            return colleges.Select(MapToResponse);
        }

        // Get pending colleges
        public async Task<IEnumerable<CollegeResponse>> GetPendingAsync()
        {
            var colleges = await _repository.GetPendingAsync();

            return colleges.Select(MapToResponse);
        }

        // Get college by ID
        public async Task<CollegeResponse?> GetByIdAsync(int id)
        {
            var college = await _repository.GetByIdAsync(id);

            return college == null ? null : MapToResponse(college);
        }

        // Register college
        public async Task<CollegeResponse> RegisterAsync(
            RegisterCollegeRequest request)
        {
            string collegeCode =
                request.CollegeCode.Trim().ToUpperInvariant();

            string officialEmail =
                request.OfficialEmail.Trim().ToLowerInvariant();

            if (await _repository.CollegeCodeExistsAsync(collegeCode))
            {
                throw new InvalidOperationException(
                    "A college with this college code already exists.");
            }

            if (await _repository.OfficialEmailExistsAsync(officialEmail))
            {
                throw new InvalidOperationException(
                    "A college with this official email already exists.");
            }

            var college = new College
            {
                CollegeName = request.CollegeName.Trim(),
                CollegeCode = collegeCode,
                UniversityName = request.UniversityName?.Trim(),
                OfficialEmail = officialEmail,
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address.Trim(),
                Website = request.Website?.Trim(),
                Status = CollegeStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(college);
            await _repository.SaveChangesAsync();

            return MapToResponse(college);
        }

        // Update college
        public async Task<CollegeResponse?> UpdateAsync(
            int id,
            UpdateCollegeRequest request)
        {
            var college = await _repository.GetByIdAsync(id);

            if (college == null)
                return null;

            string collegeCode =
                request.CollegeCode.Trim().ToUpperInvariant();

            string officialEmail =
                request.OfficialEmail.Trim().ToLowerInvariant();

            if (await _repository.CollegeCodeExistsAsync(collegeCode, id))
            {
                throw new InvalidOperationException(
                    "Another college already uses this college code.");
            }

            if (await _repository.OfficialEmailExistsAsync(officialEmail, id))
            {
                throw new InvalidOperationException(
                    "Another college already uses this official email.");
            }

            college.CollegeName = request.CollegeName.Trim();
            college.CollegeCode = collegeCode;
            college.UniversityName = request.UniversityName?.Trim();
            college.OfficialEmail = officialEmail;
            college.PhoneNumber = request.PhoneNumber.Trim();
            college.Address = request.Address.Trim();
            college.Website = request.Website?.Trim();

            _repository.Update(college);
            await _repository.SaveChangesAsync();

            return MapToResponse(college);
        }

        // Approve, reject or suspend college
        public async Task<bool> ChangeStatusAsync(
            int id,
            CollegeStatus status)
        {
            var college = await _repository.GetByIdAsync(id);

            if (college == null)
                return false;

            if (!Enum.IsDefined(typeof(CollegeStatus), status))
            {
                throw new ArgumentException(
                    "Invalid college status.");
            }

            if (status == CollegeStatus.Pending)
            {
                throw new ArgumentException(
                    "College status cannot be changed back to Pending.");
            }

            if (college.Status == CollegeStatus.Inactive)
            {
                throw new InvalidOperationException(
                    "Inactive colleges cannot change status.");
            }

            college.Status = status;

            _repository.Update(college);
            await _repository.SaveChangesAsync();

            return true;
        }

        // Map entity to response DTO
        private static CollegeResponse MapToResponse(College college)
        {
            return new CollegeResponse
            {
                CollegeId = college.CollegeId,
                CollegeName = college.CollegeName,
                CollegeCode = college.CollegeCode,
                UniversityName = college.UniversityName,
                OfficialEmail = college.OfficialEmail,
                PhoneNumber = college.PhoneNumber,
                Address = college.Address,
                Website = college.Website,
                Status = college.Status,
                CreatedAt = college.CreatedAt
            };
        }
    }
}
