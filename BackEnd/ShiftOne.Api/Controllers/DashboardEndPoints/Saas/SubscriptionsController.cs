using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Subscriptions;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/subscriptions")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ICompanySubscriptionService _service;
        public SubscriptionsController(ICompanySubscriptionService service) => _service = service;

        /// <summary>Get company subscriptions with optional company filter.</summary>
        [HasPermission(Permissions.Subscriptions.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? companyId) => (await _service.GetAllAsync(request, companyId)).ToActionResult();

        /// <summary>Assign a subscription plan to a company.</summary>
        [HasPermission(Permissions.Subscriptions.Create)]
        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignCompanySubscriptionRequest request) => (await _service.AssignAsync(request)).ToActionResult();
    }
}