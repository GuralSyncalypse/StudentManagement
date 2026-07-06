using LuongChiHai_QLSV.Server.Entities;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IRepository<User> Users { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Role> Roles { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
