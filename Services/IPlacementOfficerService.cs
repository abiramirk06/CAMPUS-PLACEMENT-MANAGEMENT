using CampusPlacementManagement.DTOs.PlacementOfficer;

namespace CampusPlacementManagement.Services
{
    public interface IPlacementOfficerService
    {
        Task<PlacementOfficerResponse> RegisterAsync(
            RegisterPlacementOfficerRequest request);

        Task<IEnumerable<PlacementOfficerResponse>> GetAllAsync();

        Task<IEnumerable<PlacementOfficerResponse>> GetPendingAsync();

        Task<PlacementOfficerResponse?> GetByIdAsync(int id);

        Task<PlacementOfficerResponse?> GetMyProfileAsync(string userId);

        Task<bool> ApproveAsync(int id, string approvedBy);

        Task<bool> RejectAsync(int id);

        Task<bool> SuspendAsync(int id);
    }
}