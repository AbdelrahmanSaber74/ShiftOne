namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IReportQueryBuilder<TSource>
    {
        IQueryable<TSource> ApplySearch(IQueryable<TSource> query, string? keyword);
        IQueryable<TSource> ApplySorting(IQueryable<TSource> query, string? sortBy, bool descending);
    }
}
