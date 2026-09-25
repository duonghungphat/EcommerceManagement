using EcommerceManagement.Core.Models;
using System.Linq.Expressions;

namespace EcommerceManagement.Data.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> BuildQuery(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
