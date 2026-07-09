using LuongChiHai_QLSV.Server.Entities;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<List<Enrollment>> GetAllWithDetailsAsync();

        Task<Enrollment?> GetByIdWithDetailsAsync(int enrollmentId);

        Task<List<Enrollment>> GetByStudentWithDetailsAsync(string studentId);

        Task<Enrollment?> GetBySectionAndStudentAsync(int sectionId, string studentId);

        Task<bool> ExistsAsync(
            int sectionId,
            string studentId);

        Task<int> CountBySectionAsync(
            int sectionId);

        Task<List<Enrollment>> GetByStudentAsync(
            string studentId);
    }
}
