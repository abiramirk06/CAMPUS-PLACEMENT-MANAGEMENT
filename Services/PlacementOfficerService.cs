using CampusPlacementManagement.DTOs.PlacementOfficer;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services
{
    public class PlacementOfficerService : IPlacementOfficerService
    {
        private readonly IPlacementOfficerRepository _repository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PlacementOfficerService(
            IPlacementOfficerRepository repository,
            ICollegeRepository collegeRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _repository = repository;
            _collegeRepository = collegeRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<PlacementOfficerResponse> RegisterAsync(
            RegisterPlacementOfficerRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                throw new InvalidOperationException(
                    "An account with this email already exists.");
            }

            var college = await _collegeRepository.GetByIdAsync(
                request.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "The selected college does not exist or is not approved.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                EmailConfirmed = false
            };

            var userResult = await _userManager.CreateAsync(
                user, request.Password);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(" ", userResult.Errors.Select(e => e.Description)));
            }

            var officer = new PlacementOfficer
            {
                UserId = user.Id,
                CollegeId = request.CollegeId,
                Designation = request.Designation.Trim(),
                Status = PlacementOfficerStatus.Pending
            };

            try
            {
                await _repository.AddAsync(officer);
                await _repository.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            var savedOfficer = await _repository.GetByIdAsync(
                officer.PlacementOfficerId);

            return MapToResponse(savedOfficer!);
        }

        public async Task<IEnumerable<PlacementOfficerResponse>> GetAllAsync()
        {
            var officers = await _repository.GetAllAsync();
            return officers.Select(MapToResponse);
        }

        public async Task<IEnumerable<PlacementOfficerResponse>> GetPendingAsync()
        {
            var officers = await _repository.GetPendingAsync();
            return officers.Select(MapToResponse);
        }

        public async Task<PlacementOfficerResponse?> GetByIdAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);
            return officer == null ? null : MapToResponse(officer);
        }

        public async Task<PlacementOfficerResponse?> GetMyProfileAsync(
            string userId)
        {
            var officer = await _repository.GetByUserIdAsync(userId);
            return officer == null ? null : MapToResponse(officer);
        }

        public async Task<bool> ApproveAsync(int id, string approvedBy)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
                return false;

            if (officer.Status != PlacementOfficerStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending placement officers can be approved.");
            }

            const string roleName = "PlacementOfficer";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!createRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(" ", createRoleResult.Errors.Select(e => e.Description)));
                }
            }

            if (officer.User == null)
            {
                throw new InvalidOperationException(
                    "Placement officer user account was not found.");
            }

            if (!await _userManager.IsInRoleAsync(officer.User, roleName))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(
                    officer.User, roleName);

                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(" ", addRoleResult.Errors.Select(e => e.Description)));
                }
            }

            officer.Status = PlacementOfficerStatus.Approved;
            officer.ApprovedBy = approvedBy;
            officer.ApprovedAt = DateTime.UtcNow;

            _repository.Update(officer);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
                return false;

            if (officer.Status != PlacementOfficerStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending placement officers can be rejected.");
            }

            officer.Status = PlacementOfficerStatus.Rejected;

            _repository.Update(officer);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SuspendAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
                return false;

            if (officer.Status != PlacementOfficerStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved placement officers can be suspended.");
            }

            officer.Status = PlacementOfficerStatus.Suspended;

            _repository.Update(officer);
            await _repository.SaveChangesAsync();

            return true;
        }

        private static PlacementOfficerResponse MapToResponse(
            PlacementOfficer officer)
        {
            return new PlacementOfficerResponse
            {
                PlacementOfficerId = officer.PlacementOfficerId,
                FullName = officer.User?.FullName ?? string.Empty,
                Email = officer.User?.Email ?? string.Empty,
                PhoneNumber = officer.User?.PhoneNumber ?? string.Empty,
                CollegeId = officer.CollegeId,
                CollegeName = officer.College?.CollegeName ?? string.Empty,
                Designation = officer.Designation,
                Status = officer.Status,
                ApprovedBy = officer.ApprovedBy,
                ApprovedAt = officer.ApprovedAt
            };
        }
    }
}