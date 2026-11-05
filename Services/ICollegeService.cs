using CampusPlacementManagement.DTOs.College;

namespace CampusPlacementManagement.Services
{
    public interface ICollegeService
    {
        Task<IEnumerable<CollegeResponse>> GetAllAsync();

        Task<IEnumerable<CollegeResponse>> GetPendingAsync();

        Task<CollegeResponse?> GetByIdAsync(int id);

        Task<CollegeResponse> RegisterAsync(RegisterCollegeRequest request);

        Task<CollegeResponse?> UpdateAsync(
            int id,
            UpdateCollegeRequest request);

        Task<bool> ChangeStatusAsync(
            int id,
            CampusPlacementManagement.Enums.CollegeStatus status);
    }
}