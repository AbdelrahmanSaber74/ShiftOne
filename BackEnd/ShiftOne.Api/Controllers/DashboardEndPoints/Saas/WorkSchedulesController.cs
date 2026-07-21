using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.WorkSchedules;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/work-schedules")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class WorkSchedulesController : ControllerBase
    {
        private readonly IWorkScheduleService _service;
        public WorkSchedulesController(IWorkScheduleService service) => _service = service;

        /// <summary>Get work schedules with optional company, branch, status, search, and pagination filters.</summary>
        [HasPermission(Permissions.WorkSchedules.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword, [FromQuery] Guid? companyId, [FromQuery] Guid? branchId, [FromQuery] bool? isActive) =>
            (await _service.GetAllAsync(request, keyword, companyId, branchId, isActive)).ToActionResult();

        /// <summary>Get work schedule details including weekly day rules.</summary>
        [HasPermission(Permissions.WorkSchedules.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a work schedule for a company or branch.</summary>
        [HasPermission(Permissions.WorkSchedules.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertWorkScheduleRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update work schedule details and weekly day rules.</summary>
        [HasPermission(Permissions.WorkSchedules.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertWorkScheduleRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Deactivate a work schedule.</summary>
        [HasPermission(Permissions.WorkSchedules.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();

        /// <summary>Set a work schedule as the active company default schedule.</summary>
        [HasPermission(Permissions.WorkSchedules.Assign)]
        [HttpPost("{id:guid}/set-default")]
        public async Task<IActionResult> SetDefault(Guid id) => (await _service.SetDefaultAsync(id)).ToActionResult();
    }
}
