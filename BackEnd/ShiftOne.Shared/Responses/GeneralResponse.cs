using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ShiftOne.Shared.Responses
{
    public class GeneralResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public object? Data { get; set; }

        [JsonIgnore]
        public Dictionary<string, string>? MessagePlaceholders { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Page { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PageSize { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalCount { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalPages => (PageSize.HasValue && TotalCount.HasValue && PageSize > 0)
                                    ? (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value)
                                    : null;

        public GeneralResponse(string? message = null, bool success = true, int statusCode = 200, object? data = null)
        {
            Message = message ?? (success ? "Success" : "Failure");
            Success = success;
            StatusCode = statusCode;
            Data = data;
        }

        public static GeneralResponse Ok(string message = "Success", object? data = null,
                                         int? page = null, int? pageSize = null, int? totalCount = null)
        {
            return new GeneralResponse(message, true, 200, data)
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public static GeneralResponse BadRequest(string message = "Bad Request", object? data = null) =>
            new GeneralResponse(message, false, 400, data);

        public static GeneralResponse NotFound(string message = "Not Found", object? data = null) =>
            new GeneralResponse(message, false, 404, data);

        public static GeneralResponse Unauthorized(string message = "Unauthorized", object? data = null) =>
            new GeneralResponse(message, false, 401, data);

        public static GeneralResponse InternalError(string message = "Internal Server Error", object? data = null) =>
            new GeneralResponse(message, false, 500, data);
    }

}


