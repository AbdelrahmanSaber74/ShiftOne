namespace ShiftOne.Core.Entities.Contracts
{
    public interface IBaseEntity<TId>
    {
        TId Id { get; set; }
    }
}


