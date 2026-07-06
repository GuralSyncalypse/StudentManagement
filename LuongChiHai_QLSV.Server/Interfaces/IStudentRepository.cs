using LuongChiHai_QLSV.Server.Entities;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<List<Student>> GetAllWithProfileAsync();
        Task<Student?> GetByIdWithProfileAsync(string id);
    }
}
