using LuongChiHai_QLSV.Server.DTOs.Students;

namespace LuongChiHai_QLSV.Server.Services
{
    public interface IStudentService
    {
        Task CreateStudentAccountAsync(StudentRequestDto dto);
        Task<List<StudentResponseDto>> GetAllAsync();
        Task<StudentResponseDto?> GetByIdAsync(string id);
        Task UpdateAsync(string id, StudentRequestDto dto);
        Task DeleteAsync(string id);
    }
}
