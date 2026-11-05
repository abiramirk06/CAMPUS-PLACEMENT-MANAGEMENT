using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories
{
    public interface IPlacementOfficerRepository
    {
        Task<IEnumerable<PlacementOfficer>> GetAllAsync();

        Task<IEnumerable<PlacementOfficer>> GetPendingAsync();

        Task<PlacementOfficer?> GetByIdAsync(int id);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task AddAsync(PlacementOfficer placementOfficer);

        void Update(PlacementOfficer placementOfficer);

        Task SaveChangesAsync();
    }
}
