using System.Linq.Expressions;

namespace LuongChiHai_QLSV.Server.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);

        // 🔥 Thêm dòng này để bên ngoài Service gọi được FindAsync
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // 🌟 Nên thêm cả hàm này để tìm ĐÚNG 1 bản ghi (như tìm Role, tìm User)
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
