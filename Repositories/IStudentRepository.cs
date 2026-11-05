
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<IEnumerable<Student>> GetByCollegeIdAsync(int collegeId);
        Task<IEnumerable<Student>> GetPendingByCollegeIdAsync(int collegeId);
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByUserIdAsync(string userId);
        Task<bool> RegisterNumberExistsAsync(int collegeId, string registerNumber);
        Task AddAsync(Student student);
        void Update(Student student);
        Task SaveChangesAsync();
    }
}
