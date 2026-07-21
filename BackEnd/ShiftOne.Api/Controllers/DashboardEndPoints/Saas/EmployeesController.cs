using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Devices;
using ShiftOne.Shared.Requests.Employees;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/employees")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeManagementService _service;
        public EmployeesController(IEmployeeManagementService service) => _service = service;

        /// <summary>Get a paginated list of employees with optional search, status, and branch filters.</summary>
        [HasPermission(Permissions.Employees.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword, [FromQuery] bool? isActive, [FromQuery] Guid? branchId) => (await _service.GetAllAsync(request, keyword, isActive, branchId)).ToActionResult();

        /// <summary>Get employee details by employee id.</summary>
        [HasPermission(Permissions.Employees.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create an employee account under the current company context.</summary>
        [HasPermission(Permissions.Employees.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update employee profile, branch assignment, and active status.</summary>
        [HasPermission(Permissions.Employees.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Deactivate an employee account.</summary>
        [HasPermission(Permissions.Employees.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();

        /// <summary>Reset an employee device binding so the next login can bind a new device.</summary>
        [HasPermission(Permissions.Devices.Reset)]
        [HttpPost("reset-device")]
        public async Task<IActionResult> ResetDevice([FromBody] ResetEmployeeDeviceRequest request) => (await _service.ResetDeviceAsync(request.EmployeeId)).ToActionResult();
    }
}