namespace ShiftOne.Core.Interfaces.Infrastructure.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid Id);
        Task<IEnumerable<T>> GetAllAsync(ISpecification<T>? specification = null);
        Task<int> CountAsync(ISpecification<T>? specification = null);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAsync();
    }
}


