using ShiftOne.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ShiftOne.Api.Extensions
{
    public static class GeneralResponseExtensions
    {
        public static IActionResult ToActionResult(this GeneralResponse response)
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
        }
    }
}
