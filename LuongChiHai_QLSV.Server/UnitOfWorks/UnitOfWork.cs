using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using LuongChiHai_QLSV.Server.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LuongChiHai_QLSV.Server.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SchoolContext _context;
        private IDbContextTransaction? _transaction;

        public IStudentRepository Students { get; private set; }
        public IRepository<User> Users { get; private set; }
        public IRepository<UserRole> UserRoles { get; private set; }
        public IRepository<Role> Roles { get; private set; }

        public UnitOfWork(SchoolContext context)
        {
            _context = context;
            Students = new StudentRepository(_context);
            Users = new Repository<User>(_context);
            UserRoles = new Repository<UserRole>(_context);
            Roles = new Repository<Role>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null) await _transaction.CommitAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null) await _transaction.RollbackAsync();
        }

        public void Dispose() => _context.Dispose();
    }
}
