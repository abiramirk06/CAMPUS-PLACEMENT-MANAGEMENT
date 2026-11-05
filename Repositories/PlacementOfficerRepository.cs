using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Repositories
{
    public class PlacementOfficerRepository : IPlacementOfficerRepository
    {
        private readonly ApplicationDbContext _context;

        public PlacementOfficerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlacementOfficer>> GetAllAsync()
        {
            return await _context.PlacementOfficers
                .AsNoTracking()
                .Include(po => po.User)
                .Include(po => po.College)
                .Include(po => po.ApprovedByUser)
                .OrderBy(po => po.PlacementOfficerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PlacementOfficer>> GetPendingAsync()
        {
            return await _context.PlacementOfficers
                .AsNoTracking()
                .Include(po => po.User)
                .Include(po => po.College)
                .Where(po => po.Status ==
                    PlacementOfficerStatus.Pending)
                .OrderBy(po => po.PlacementOfficerId)
                .ToListAsync();
        }

        public async Task<PlacementOfficer?> GetByIdAsync(int id)
        {
            return await _context.PlacementOfficers
                .Include(po => po.User)
                .Include(po => po.College)
                .Include(po => po.ApprovedByUser)
                .FirstOrDefaultAsync(
                    po => po.PlacementOfficerId == id);
        }

        public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
        {
            return await _context.PlacementOfficers
                .AsNoTracking()
                .Include(po => po.User)
                .Include(po => po.College)
                .FirstOrDefaultAsync(po => po.UserId == userId);
        }

        public async Task AddAsync(PlacementOfficer placementOfficer)
        {
            await _context.PlacementOfficers.AddAsync(placementOfficer);
        }

        public void Update(PlacementOfficer placementOfficer)
        {
            _context.PlacementOfficers.Update(placementOfficer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}