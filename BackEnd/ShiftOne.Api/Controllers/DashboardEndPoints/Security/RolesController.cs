using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Roles;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Security
{
    [Route("api/dashboard/roles")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleManagementService _service;
        public RolesController(IRoleManagementService service) => _service = service;

        /// <summary>Get a paginated list of roles with optional search and status filters.</summary>
        [HasPermission(Permissions.Roles.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword, [FromQuery] bool? isActive) => (await _service.GetAllAsync(request, keyword, isActive)).ToActionResult();

        /// <summary>Get role details and assigned permissions by role id.</summary>
        [HasPermission(Permissions.Roles.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a custom dashboard role.</summary>
        [HasPermission(Permissions.Roles.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertRoleRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update a role name, description, and active status.</summary>
        [HasPermission(Permissions.Roles.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertRoleRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Deactivate or remove a role when it is not protected.</summary>
        [HasPermission(Permissions.Roles.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();

        /// <summary>Replace the permissions assigned to a role.</summary>
        [HasPermission(Permissions.Roles.Edit)]
        [HttpPut("{id:guid}/permissions")]
        public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignRolePermissionsRequest request) => (await _service.AssignPermissionsAsync(id, request)).ToActionResult();
    }
}