using System.Linq.Expressions;

namespace ShiftOne.Core.Interfaces.Infrastructure.Repositories
{
    public interface ISpecification<T>
        where T : class
    {
        /// <summary>
        /// Main filtering criteria for the query.
        /// </summary>
        Expression<Func<T, bool>>? Criteria { get; }

        /// <summary>
        /// Legacy includes: simple navigation property includes.
        /// Example: x => x.Customer
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// New style includes: full Include/ThenInclude chains as query transformers.
        /// Example: q => q.Include(x => x.Customer).ThenInclude(c => c.Addresses)
        /// </summary>
        List<Func<IQueryable<T>, IQueryable<T>>> IncludeChains { get; }

        /// <summary>
        /// Order ascending.
        /// </summary>
        Expression<Func<T, object>>? OrderBy { get; }

        /// <summary>
        /// Order descending.
        /// </summary>
        Expression<Func<T, object>>? OrderByDescending { get; }

        /// <summary>
        /// Number of records to skip.
        /// </summary>
        int? Skip { get; }

        /// <summary>
        /// Number of records to take.
        /// </summary>
        int? Take { get; }
    }
}


