using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EcommerceManagement.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> BuildQuery(Expression<Func<T, bool>> predicate)
        {
            return _dbSet
                .AsNoTracking()
                .Where(predicate);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}