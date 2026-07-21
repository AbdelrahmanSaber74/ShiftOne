using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace ShiftOne.Api.Filters
{
    public class AcceptLanguageHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Response language. Use 'en' or 'ar'. Defaults to 'en'.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("en"),
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("en"),
                        new OpenApiString("ar")
                    }
                }
            });
        }
    }
}
