using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ShiftOne.Api.Filters
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum)
                return;

            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type).Cast<int>().ToList();

            schema.Description ??= "";
            schema.Description += "Possible values:\n";

            foreach (var (name, value) in enumNames.Zip(enumValues, (n, v) => (n, v)))
            {
                schema.Description += $"{name} = {value},\n";
            }
        }
    }

}

