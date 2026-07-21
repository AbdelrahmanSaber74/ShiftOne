using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Logging
{
    public class LogEntry : IBaseEntity<int>
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? Properties { get; set; }
        public string? CorrelationId { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestPath { get; set; }
        public int? StatusCode { get; set; }
        public long? ExecutionTimeMs { get; set; }
        public string? ClientIp { get; set; }
        public string? ApplicationName { get; set; }
        public string? EnvironmentName { get; set; }
        public string? MachineName { get; set; }
    }
}
