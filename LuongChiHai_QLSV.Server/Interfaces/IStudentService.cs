using LuongChiHai_QLSV.Server.DTOs.Students;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IStudentService
    {
        Task CreateStudentAccountAsync(StudentRequestDto dto);
        Task<List<StudentResponseDto>> GetAllAsync();
        Task<StudentResponseDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, StudentRequestDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
