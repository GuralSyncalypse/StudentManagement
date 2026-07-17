using LuongChiHai_QLSV.Server.DTOs.Students;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IStudentService
    {
        Task CreateStudentAccountAsync(StudentCreateDto dto);
        Task<List<StudentListDto>> GetAllAsync();
        Task<StudentDetailDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, StudentUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
