
using CampusPlacementManagement.DTOs.Student;

namespace CampusPlacementManagement.Services
{
    public interface IStudentService
    {
        Task<StudentResponse> EnrollAsync(
            EnrollStudentRequest request, string officerUserId);

        Task<IEnumerable<StudentResponse>> BulkEnrollAsync(
            IEnumerable<EnrollStudentRequest> requests,
            string officerUserId);

        Task<IEnumerable<StudentResponse>> GetAllAsync(
            string userId, bool isSuperAdmin);

        Task<IEnumerable<StudentResponse>> GetPendingAsync(string officerUserId);

        Task<StudentResponse?> GetByIdAsync(int id);

        Task<StudentResponse?> GetMyProfileAsync(string userId);

        Task<bool> ApproveAsync(int id, string officerUserId);

        Task<bool> RejectAsync(int id, string officerUserId);

        Task<bool> SuspendAsync(int id, string userId, bool isSuperAdmin);

        Task<bool> UpdateAsync(
            int id, UpdateStudentRequest request,
            string userId, bool isSuperAdmin);
    }
}
