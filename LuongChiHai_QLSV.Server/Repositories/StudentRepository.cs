using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuongChiHai_QLSV.Server.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        // Dùng constructor này để đẩy SchoolContext lên cho lớp cha xử lý
        public StudentRepository(SchoolContext context) : base(context)
        {
        }

        // 🌟 Hàm lấy toàn bộ danh sách Sinh viên kèm theo AcademicProfile
        public async Task<List<Student>> GetAllWithProfileAsync()
        {
            // Sử dụng _dbSet (đại diện cho _context.Students) được thừa kế từ lớp cha
            return await _dbSet.Include(s => s.AcademicProfile)
                               .ToListAsync();
        }

        // 🌟 Hàm lấy chi tiết một Sinh viên theo ID kèm theo AcademicProfile
        public async Task<Student?> GetByIdWithProfileAsync(string id)
        {
            return await _dbSet.Include(s => s.AcademicProfile)
                               .FirstOrDefaultAsync(s => s.StudentID == id);
        }
    }
}
