using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iterative_Project2.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _service;

        public ApplicationsController(IApplicationService service) => _service = service;

        // NOTE: role names must match your UserRole enum values.

        // ===================== Candidate =====================

        // POST /api/applications   body: { "jobId": "...", "coverLetter": "...", "resumeUrl": "https://..." }
        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public Task<IActionResult> Apply(ApplyJobDto dto) => Run(async () =>
        {
            var created = await _service.ApplyToJob(dto);
            return CreatedAtAction(nameof(GetMine), new { id = created.ApplicationId }, created);
        });

        // GET /api/applications/mine?status=Pending&page=1&pageSize=10
        [Authorize(Roles = "Candidate")]
        [HttpGet("mine")]
        public Task<IActionResult> MyApplications([FromQuery] ApplicationFilterRequestDto filter) =>
            Run(async () => Ok(await _service.GetMyApplications(filter)));

        [Authorize(Roles = "Candidate")]
        [HttpGet("mine/{id:guid}")]
        public Task<IActionResult> GetMine(Guid id) => Run(async () =>
        {
            var result = await _service.GetMyApplication(id);
            return result is null ? NotFound() : Ok(result);
        });

        // GET /api/applications/jobs/{jobId}/applied  -> { "hasApplied": true }
        [Authorize(Roles = "Candidate")]
        [HttpGet("jobs/{jobId:guid}/applied")]
        public Task<IActionResult> HasApplied(Guid jobId) =>
            Run(async () => Ok(new { hasApplied = await _service.HasApplied(jobId) }));

        // DELETE /api/applications/mine/{id}   (only while Pending)
        [Authorize(Roles = "Candidate")]
        [HttpDelete("mine/{id:guid}")]
        public Task<IActionResult> Withdraw(Guid id) =>
            Run(async () => await _service.WithdrawApplication(id) ? NoContent() : NotFound());

        // ===================== Employer / Admin =====================

        // GET /api/applications/jobs/{jobId}?status=Pending&page=1&pageSize=10
        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("jobs/{jobId:guid}")]
        public Task<IActionResult> Applicants(Guid jobId, [FromQuery] ApplicationFilterRequestDto filter) =>
            Run(async () => Ok(await _service.GetJobApplicants(jobId, filter)));

        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("jobs/{jobId:guid}/summary")]
        public Task<IActionResult> Summary(Guid jobId) =>
            Run(async () => Ok(await _service.GetJobApplicationSummary(jobId)));

        // Full candidate profile for one application
        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("{id:guid}")]
        public Task<IActionResult> ApplicantDetail(Guid id) => Run(async () =>
        {
            var result = await _service.GetApplicantDetail(id);
            return result is null ? NotFound() : Ok(result);
        });

        // PATCH /api/applications/{id}/status   body: { "status": "Accepted" }
        [Authorize(Roles = "Employer,Admin")]
        [HttpPatch("{id:guid}/status")]
        public Task<IActionResult> UpdateStatus(Guid id, UpdateApplicationStatusDto dto) =>
            Run(async () => Ok(await _service.UpdateApplicationStatus(id, dto)));

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