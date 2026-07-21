using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Branches;
using ShiftOne.Shared.Requests.WorkSchedules;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/branches")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _service;
        private readonly IWorkScheduleService _workScheduleService;

        public BranchesController(IBranchService service, IWorkScheduleService workScheduleService)
        {
            _service = service;
            _workScheduleService = workScheduleService;
        }

        /// <summary>Get a paginated list of branches for the current company context.</summary>
        [HasPermission(Permissions.Branches.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword, [FromQuery] bool? isActive, [FromQuery] Guid? companyId) => (await _service.GetAllAsync(request, keyword, isActive, companyId)).ToActionResult();

        /// <summary>Get branch details by branch id.</summary>
        [HasPermission(Permissions.Branches.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a branch with geofence coordinates and allowed radius.</summary>
        [HasPermission(Permissions.Branches.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertBranchRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update branch details, geofence settings, and status.</summary>
        [HasPermission(Permissions.Branches.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertBranchRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Assign a work schedule override to a branch.</summary>
        [HasPermission(Permissions.WorkSchedules.Assign)]
        [HttpPost("{branchId:guid}/schedule")]
        public async Task<IActionResult> AssignSchedule(Guid branchId, [FromBody] AssignBranchScheduleRequest request) => (await _workScheduleService.AssignToBranchAsync(branchId, request)).ToActionResult();

        /// <summary>Clear branch schedule override so the branch uses the company default schedule.</summary>
        [HasPermission(Permissions.WorkSchedules.Assign)]
        [HttpDelete("{branchId:guid}/schedule")]
        public async Task<IActionResult> ClearSchedule(Guid branchId) => (await _workScheduleService.ClearBranchScheduleAsync(branchId)).ToActionResult();

        /// <summary>Deactivate a branch.</summary>
        [HasPermission(Permissions.Branches.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();
    }
}
