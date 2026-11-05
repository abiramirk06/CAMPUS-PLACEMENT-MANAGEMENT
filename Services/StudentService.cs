
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentService(
            IStudentRepository repository,
            IPlacementOfficerRepository officerRepository,
            ICollegeRepository collegeRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _repository = repository;
            _officerRepository = officerRepository;
            _collegeRepository = collegeRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<StudentResponse> EnrollAsync(
            EnrollStudentRequest request, string officerUserId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved)
                throw new InvalidOperationException(
                    "An approved placement officer account is required.");

            if (officer.College == null ||
                officer.College.Status != CollegeStatus.Approved)
                throw new InvalidOperationException(
                    "Your college is not currently approved.");

            string email = request.Email.Trim().ToLowerInvariant();
            string registerNumber = request.RegisterNumber.Trim().ToUpperInvariant();

            if (await _userManager.FindByEmailAsync(email) != null)
                throw new InvalidOperationException(
                    "An account with this email already exists.");

            if (await _repository.RegisterNumberExistsAsync(
                officer.CollegeId, registerNumber))
                throw new InvalidOperationException(
                    "This register number already exists in your college.");

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
                throw new InvalidOperationException(
                    string.Join(" ", userResult.Errors.Select(e => e.Description)));

            var student = new Student
            {
                UserId = user.Id,
                CollegeId = officer.CollegeId,
                PlacementOfficerId = officer.PlacementOfficerId,
                RegisterNumber = registerNumber,
                Department = request.Department.Trim(),
                Degree = request.Degree.Trim(),
                GraduationYear = request.GraduationYear,
                CGPA = request.CGPA,
                ActiveBacklogs = request.ActiveBacklogs,
                ResumeUrl = request.ResumeUrl,
                Status = StudentStatus.Pending
            };

            try
            {
                await _repository.AddAsync(student);
                await _repository.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            var saved = await _repository.GetByIdAsync(student.StudentId);
            return Map(saved!);
        }

        public async Task<IEnumerable<StudentResponse>> BulkEnrollAsync(
            IEnumerable<EnrollStudentRequest> requests,
            string officerUserId)
        {
            var results = new List<StudentResponse>();

            foreach (var request in requests)
            {
                results.Add(await EnrollAsync(request, officerUserId));
            }

            return results;
        }

        public async Task<IEnumerable<StudentResponse>> GetAllAsync(
            string userId, bool isSuperAdmin)
        {
            if (isSuperAdmin)
            {
                var all = await _repository.GetAllAsync();
                return all.Select(Map);
            }

            var officer = await _officerRepository.GetByUserIdAsync(userId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved)
                throw new UnauthorizedAccessException(
                    "An approved placement officer account is required.");

            var students = await _repository.GetByCollegeIdAsync(
                officer.CollegeId);

            return students.Select(Map);
        }

        public async Task<IEnumerable<StudentResponse>> GetPendingAsync(
            string officerUserId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved)
                throw new UnauthorizedAccessException(
                    "An approved placement officer account is required.");

            var students = await _repository.GetPendingByCollegeIdAsync(
                officer.CollegeId);

            return students.Select(Map);
        }

        public async Task<StudentResponse?> GetByIdAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            return student == null ? null : Map(student);
        }

        public async Task<StudentResponse?> GetMyProfileAsync(string userId)
        {
            var student = await _repository.GetByUserIdAsync(userId);
            return student == null ? null : Map(student);
        }

        public async Task<bool> ApproveAsync(int id, string officerUserId)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return false;

            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved ||
                officer.CollegeId != student.CollegeId)
                throw new UnauthorizedAccessException(
                    "You cannot approve students outside your college.");

            if (student.Status != StudentStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending students can be approved.");

            const string roleName = "Student";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            if (student.User == null)
                throw new InvalidOperationException(
                    "Student user account was not found.");

            if (!await _userManager.IsInRoleAsync(student.User, roleName))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    student.User, roleName);

                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(" ", roleResult.Errors.Select(e => e.Description)));
            }

            student.Status = StudentStatus.Approved;
            student.ApprovedAt = DateTime.UtcNow;

            _repository.Update(student);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectAsync(int id, string officerUserId)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return false;

            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved ||
                officer.CollegeId != student.CollegeId)
                throw new UnauthorizedAccessException(
                    "You cannot reject students outside your college.");

            if (student.Status != StudentStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending students can be rejected.");

            student.Status = StudentStatus.Rejected;

            _repository.Update(student);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SuspendAsync(
            int id, string userId, bool isSuperAdmin)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return false;

            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    officer.Status != PlacementOfficerStatus.Approved ||
                    officer.CollegeId != student.CollegeId)
                    throw new UnauthorizedAccessException(
                        "You cannot suspend students outside your college.");
            }

            if (student.Status != StudentStatus.Approved)
                throw new InvalidOperationException(
                    "Only approved students can be suspended.");

            student.Status = StudentStatus.Suspended;

            _repository.Update(student);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(
            int id, UpdateStudentRequest request,
            string userId, bool isSuperAdmin)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return false;

            bool isOwner = student.UserId == userId;

            if (!isSuperAdmin && !isOwner)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    officer.Status != PlacementOfficerStatus.Approved ||
                    officer.CollegeId != student.CollegeId)
                    throw new UnauthorizedAccessException(
                        "You cannot update this student's profile.");
            }

            if (student.Status == StudentStatus.Suspended)
                throw new InvalidOperationException(
                    "Suspended student profiles cannot be updated.");

            if (isOwner && !isSuperAdmin)
            {
                // Students can update personal/academic details,
                // but cannot change register number or college.
            }

            student.User!.FullName = request.FullName.Trim();
            student.User.PhoneNumber = request.PhoneNumber.Trim();
            student.Department = request.Department.Trim();
            student.Degree = request.Degree.Trim();
            student.GraduationYear = request.GraduationYear;
            student.CGPA = request.CGPA;
            student.ActiveBacklogs = request.ActiveBacklogs;
            student.ResumeUrl = request.ResumeUrl;

            _repository.Update(student);
            await _repository.SaveChangesAsync();

            return true;
        }

        private static StudentResponse Map(Student student)
        {
            return new StudentResponse
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User?.FullName ?? string.Empty,
                Email = student.User?.Email ?? string.Empty,
                PhoneNumber = student.User?.PhoneNumber ?? string.Empty,
                CollegeId = student.CollegeId,
                CollegeName = student.College?.CollegeName ?? string.Empty,
                PlacementOfficerId = student.PlacementOfficerId,
                RegisterNumber = student.RegisterNumber,
                Department = student.Department,
                Degree = student.Degree,
                GraduationYear = student.GraduationYear,
                CGPA = student.CGPA,
                ActiveBacklogs = student.ActiveBacklogs,
                ResumeUrl = student.ResumeUrl,
                Status = student.Status,
                ApprovedAt = student.ApprovedAt
            };
        }
    }
}
