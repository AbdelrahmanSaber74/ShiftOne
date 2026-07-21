using ShiftOne.Api.Constants;

namespace ShiftOne.Api.Options
{
    public sealed class LoggingOptions
    {
        public const string SectionName = "LoggingOptions";

        public string ApplicationName { get; init; } = ApiConstants.Application.Name;
        public string CorrelationHeaderName { get; init; } = ApiConstants.Headers.CorrelationId;
    }
}
