using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.Attendance;

namespace ShiftOne.Api.Controllers.UserEndPoints.User
{
    [Route("api/employees/attendance")]
    [ApiExplorerSettings(GroupName = "employees")]
    [ApiController]
    public class EmployeeAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        public EmployeeAttendanceController(IAttendanceService service) => _service = service;

        /// <summary>Check in the authenticated employee after device and geofence validation.</summary>
        [HasPermission(Permissions.Attendance.CheckIn)]
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] AttendancePunchRequest request) => (await _service.CheckInAsync(request)).ToActionResult();

        /// <summary>Check out the authenticated employee after device and geofence validation.</summary>
        [HasPermission(Permissions.Attendance.CheckOut)]
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] AttendancePunchRequest request) => (await _service.CheckOutAsync(request)).ToActionResult();

        /// <summary>Get the authenticated employee attendance history for the last three days.</summary>
        [HasPermission(Permissions.Attendance.View)]
        [HttpGet("my-history")]
        public async Task<IActionResult> MyHistory([FromQuery] int days = 3) => (await _service.GetMyHistoryAsync(days)).ToActionResult();
    }
}