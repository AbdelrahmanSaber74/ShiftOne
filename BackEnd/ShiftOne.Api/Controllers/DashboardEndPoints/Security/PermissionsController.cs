using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Permissions;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Security
{
    [Route("api/dashboard/permissions")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionManagementService _service;
        public PermissionsController(IPermissionManagementService service) => _service = service;

        /// <summary>Get a paginated list of permissions with optional search.</summary>
        [HasPermission(Permissions.PermissionManagement.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword) => (await _service.GetAllAsync(request, keyword)).ToActionResult();

        /// <summary>Get permission details by permission id.</summary>
        [HasPermission(Permissions.PermissionManagement.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a custom permission key.</summary>
        [HasPermission(Permissions.PermissionManagement.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertPermissionRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update a permission name and description when it is not protected.</summary>
        [HasPermission(Permissions.PermissionManagement.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPermissionRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Delete a custom permission when it is not protected or assigned.</summary>
        [HasPermission(Permissions.PermissionManagement.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();
    }
}