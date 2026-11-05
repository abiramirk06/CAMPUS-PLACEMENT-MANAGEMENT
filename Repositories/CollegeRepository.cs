using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class CollegeRepository : ICollegeRepository
    {
        private readonly ApplicationDbContext _context;

        public CollegeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<College>> GetAllAsync()
        {
            return await _context.Colleges
                .AsNoTracking()
                .OrderBy(c => c.CollegeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<College>> GetPendingAsync()
        {
            return await _context.Colleges
                .AsNoTracking()
                .Where(c => c.Status == CollegeStatus.Pending)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<College?> GetByIdAsync(int id)
        {
            return await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == id);
        }

        public async Task<bool> CollegeCodeExistsAsync(
            string collegeCode,
            int? excludeCollegeId = null)
        {
            return await _context.Colleges.AnyAsync(c =>
                c.CollegeCode == collegeCode &&
                (!excludeCollegeId.HasValue ||
                 c.CollegeId != excludeCollegeId.Value));
        }

        public async Task AddAsync(College college)
        {
            await _context.Colleges.AddAsync(college);
        }

        public void Update(College college)
        {
            _context.Colleges.Update(college);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> OfficialEmailExistsAsync(
    string officialEmail,
    int? excludeCollegeId = null)
        {
            return await _context.Colleges.AnyAsync(c =>
                c.OfficialEmail == officialEmail &&
                (!excludeCollegeId.HasValue ||
                 c.CollegeId != excludeCollegeId.Value));
        }
    }
}