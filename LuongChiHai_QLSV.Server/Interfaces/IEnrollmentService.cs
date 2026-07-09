using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IEnrollmentService
    {
        Task<List<EnrollmentDto>> GetMyEnrollmentsAsync(string studentId);

        Task<List<EnrollmentDto>> GetAllEnrollmentsAsync();

        Task<EnrollmentDto?> GetEnrollmentAsync(int id);

        Task<EnrollmentDto> RegisterCourseAsync(string studentId, StudentRegistrationDto dto);

        Task<EnrollmentDto> RegisterForStudentAsync(AdminRegistrationDto dto);

        Task<EnrollmentDto> UpdateEnrollmentAsync(int enrollmentId, Enrollment enrollment);

        Task DeleteEnrollmentAsync(int id);

        Task CancelCourseAsync(string studentId, int sectionId);

        Task AdminCancelEnrollmentAsync(int sectionId, string studentId);

        Task<int> GetCurrentEnrollmentCountAsync(string studentId);
    }
}
