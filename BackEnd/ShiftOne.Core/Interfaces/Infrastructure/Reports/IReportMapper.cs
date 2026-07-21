namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IReportMapper<in TSource, out TRow>
    {
        TRow Map(TSource source);
    }
}
