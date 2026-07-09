using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuongChiHai_QLSV.Server.Repositories
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(SchoolContext context) : base(context)
        {
        }

        public async Task<List<Enrollment>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.Scores)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByIdWithDetailsAsync(int enrollmentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.Scores)
                .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentId);
        }

        public async Task<List<Enrollment>> GetByStudentWithDetailsAsync(string studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.Scores)
                .Where(e => e.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetBySectionAndStudentAsync(int sectionId, string studentId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.SectionID == sectionId && e.StudentID == studentId);
        }

        public async Task<bool> ExistsAsync(int sectionId, string studentId)
        {
            return await _dbSet.AnyAsync(e => e.SectionID == sectionId && e.StudentID == studentId);
        }

        public async Task<int> CountBySectionAsync(int sectionId)
        {
            return await _dbSet.CountAsync(e => e.SectionID == sectionId);
        }

        public async Task<List<Enrollment>> GetByStudentAsync(string studentId)
        {
            return await _dbSet.Where(e => e.StudentID == studentId).ToListAsync();
        }
    }
}
