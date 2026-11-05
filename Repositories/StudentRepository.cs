
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Student> StudentQuery()
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await StudentQuery()
                .AsNoTracking()
                .OrderBy(s => s.StudentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByCollegeIdAsync(int collegeId)
        {
            return await StudentQuery()
                .AsNoTracking()
                .Where(s => s.CollegeId == collegeId)
                .OrderBy(s => s.StudentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetPendingByCollegeIdAsync(int collegeId)
        {
            return await StudentQuery()
                .AsNoTracking()
                .Where(s => s.CollegeId == collegeId &&
                            s.Status == StudentStatus.Pending)
                .OrderBy(s => s.StudentId)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await StudentQuery()
                .FirstOrDefaultAsync(s => s.StudentId == id);
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await StudentQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> RegisterNumberExistsAsync(
            int collegeId, string registerNumber)
        {
            return await _context.Students.AnyAsync(s =>
                s.CollegeId == collegeId &&
                s.RegisterNumber == registerNumber);
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public void Update(Student student)
        {
            _context.Students.Update(student);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
