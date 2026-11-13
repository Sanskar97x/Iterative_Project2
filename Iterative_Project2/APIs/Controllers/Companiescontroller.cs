using Iterative_Project2.Application.DTOs.CompanyDto;
using Iterative_Project2.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iterative_Project2.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _service;

        public CompaniesController(ICompanyService service) => _service = service;

        // NOTE: role names must match your UserRole enum values.

        // ===================== Public =====================

        // GET /api/companies?search=tech&location=Kathmandu&verifiedOnly=true&page=1&pageSize=12
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] CompanyFilterRequestDto filter) =>
            Ok(await _service.SearchCompanies(filter));

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var company = await _service.GetCompanyById(id);
            return company is null ? NotFound() : Ok(company);
        }

        // Open jobs of a company
        [AllowAnonymous]
        [HttpGet("{id:guid}/jobs")]
        public async Task<IActionResult> Jobs(Guid id)
        {
            var jobs = await _service.GetCompanyJobs(id);
            return jobs is null ? NotFound() : Ok(jobs);
        }

        // ===================== Employer =====================

        [Authorize(Roles = "Employer")]
        [HttpPost]
        public Task<IActionResult> Register(RegisterCompanyDto dto) => Run(async () =>
        {
            var created = await _service.RegisterCompany(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        });

        [Authorize(Roles = "Employer")]
        [HttpGet("mine")]
        public Task<IActionResult> Mine() => Run(async () => Ok(await _service.GetMyCompanies()));

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{id:guid}")]
        public Task<IActionResult> Update(Guid id, UpdateCompanyDto dto) =>
            Run(async () => Ok(await _service.UpdateCompany(id, dto)));

        [Authorize(Roles = "Employer,Admin")]
        [HttpDelete("{id:guid}")]
        public Task<IActionResult> Delete(Guid id) =>
            Run(async () => await _service.DeleteCompany(id) ? NoContent() : NotFound());

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost("{id:guid}/deactivate")]
        public Task<IActionResult> Deactivate(Guid id) =>
            Run(async () => Ok(await _service.SetCompanyActive(id, false)));

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost("{id:guid}/activate")]
        public Task<IActionResult> Activate(Guid id) =>
            Run(async () => Ok(await _service.SetCompanyActive(id, true)));

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost("{id:guid}/transfer-ownership")]
        public Task<IActionResult> Transfer(Guid id, TransferOwnershipDto dto) =>
            Run(async () => Ok(await _service.TransferOwnership(id, dto)));

        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("{id:guid}/stats")]
        public Task<IActionResult> Stats(Guid id) => Run(async () => Ok(await _service.GetCompanyStats(id)));

        // ===================== Admin =====================

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/verify")]
        public Task<IActionResult> Verify(Guid id) => Run(async () => Ok(await _service.SetVerified(id, true)));

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/unverify")]
        public Task<IActionResult> Unverify(Guid id) => Run(async () => Ok(await _service.SetVerified(id, false)));

        // ---------- helper ----------
        private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
        {
            try { return await action(); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}