using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Plans;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/plans")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;
        public PlansController(ISubscriptionPlanService service) => _service = service;

        /// <summary>Get a paginated list of subscription plans with optional status filter.</summary>
        [HasPermission(Permissions.Plans.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] bool? isActive) => (await _service.GetAllAsync(request, isActive)).ToActionResult();

        /// <summary>Get subscription plan details by plan id.</summary>
        [HasPermission(Permissions.Plans.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a subscription plan with feature limits.</summary>
        [HasPermission(Permissions.Plans.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertSubscriptionPlanRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update subscription plan pricing, limits, and status.</summary>
        [HasPermission(Permissions.Plans.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertSubscriptionPlanRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Deactivate a subscription plan.</summary>
        [HasPermission(Permissions.Plans.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();
    }
}