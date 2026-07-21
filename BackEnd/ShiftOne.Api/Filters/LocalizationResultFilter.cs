using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Shared.Responses;
using System.Threading.Tasks;

namespace ShiftOne.Api.Filters
{
    public class LocalizationResultFilter : IAsyncResultFilter
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationResultFilter(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is GeneralResponse response)
            {
                if (!string.IsNullOrWhiteSpace(response.Message))
                {
                    response.Message = _localizationService.GetString(response.Message, response.MessagePlaceholders);
                }
            }

            await next();
        }
    }
}
