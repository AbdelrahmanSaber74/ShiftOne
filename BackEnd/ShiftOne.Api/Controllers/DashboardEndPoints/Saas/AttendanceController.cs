using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/attendance")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        public AttendanceController(IAttendanceService service) => _service = service;

        /// <summary>Get attendance records with optional employee, branch, and date range filters.</summary>
        [HasPermission(Permissions.Attendance.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId, [FromQuery] Guid? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to) => (await _service.GetAllAsync(request, employeeId, branchId, from, to)).ToActionResult();
    }
}