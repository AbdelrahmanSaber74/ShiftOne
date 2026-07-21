using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Companies;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Saas
{
    [Route("api/dashboard/companies")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _service;
        public CompaniesController(ICompanyService service) => _service = service;

        /// <summary>Get a paginated list of companies with optional search and status filters.</summary>
        [HasPermission(Permissions.Companies.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] string? keyword, [FromQuery] bool? isActive) => (await _service.GetAllAsync(request, keyword, isActive)).ToActionResult();

        /// <summary>Get company details by company id.</summary>
        [HasPermission(Permissions.Companies.View)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id) => (await _service.GetByIdAsync(id)).ToActionResult();

        /// <summary>Create a new company and optionally create its first company admin.</summary>
        [HasPermission(Permissions.Companies.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request) => (await _service.CreateAsync(request)).ToActionResult();

        /// <summary>Update company profile and active status.</summary>
        [HasPermission(Permissions.Companies.Edit)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request) => (await _service.UpdateAsync(id, request)).ToActionResult();

        /// <summary>Deactivate a company.</summary>
        [HasPermission(Permissions.Companies.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) => (await _service.DeleteAsync(id)).ToActionResult();
    }
}