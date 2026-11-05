using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories
{
    public interface ICollegeRepository
    {
        Task<IEnumerable<College>> GetAllAsync();

        Task<IEnumerable<College>> GetPendingAsync();

        Task<College?> GetByIdAsync(int id);

        Task<bool> CollegeCodeExistsAsync(
            string collegeCode,
            int? excludeCollegeId = null);

        Task<bool> OfficialEmailExistsAsync(
            string officialEmail,
            int? excludeCollegeId = null);

        Task AddAsync(College college);

        void Update(College college);

        Task SaveChangesAsync();


    }
}