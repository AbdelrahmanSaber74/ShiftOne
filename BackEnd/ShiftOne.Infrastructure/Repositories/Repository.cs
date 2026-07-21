using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Infrastructure.Persistence;

namespace ShiftOne.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(ISpecification<T>? specification = null)
        {
            var query = ApplySpecification(_dbSet, specification, includePaging: true);
            return await query.ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T>? specification = null)
        {
            var query = ApplySpecification(_dbSet, specification, includePaging: false);
            return await query.CountAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private static IQueryable<T> ApplySpecification(
            IQueryable<T> query,
            ISpecification<T>? specification,
            bool includePaging)
        {
            if (specification == null)
                return query;

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            foreach (var include in specification.Includes)
                query = query.Include(include);

            foreach (var chain in specification.IncludeChains)
                query = chain(query);

            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);

            if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            if (!includePaging)
                return query;

            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);

            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);

            return query;
        }
    }
}
