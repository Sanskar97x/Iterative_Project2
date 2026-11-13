using Iterative_Project2.Application.DTOs.ResumeDto;
using Iterative_Project2.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.APIs.Controllers
{
        [ApiController]
        [Authorize]
        [Route("api/resume")]
        public class ResumeController : ControllerBase
        {
            private readonly IResumeService _service;

            public ResumeController(IResumeService service) => _service = service;

            // NOTE: role names must match your UserRole enum values.

            // POST /api/resume   (multipart/form-data, field name "file")
            [Authorize(Roles = "Candidate")]
            [HttpPost]
            [Consumes("multipart/form-data")]
            [RequestSizeLimit(6 * 1024 * 1024)]
            public Task<IActionResult> Upload(IFormFile file) =>
                Run(async () => Ok(await _service.UploadResume(ToDto(file))));

            // PUT /api/resume    (replace the current resume)
            [Authorize(Roles = "Candidate")]
            [HttpPut]
            [Consumes("multipart/form-data")]
            [RequestSizeLimit(6 * 1024 * 1024)]
            public Task<IActionResult> Update(IFormFile file) =>
                Run(async () => Ok(await _service.UpdateResume(ToDto(file))));

            [Authorize(Roles = "Candidate")]
            [HttpGet("me")]
            public Task<IActionResult> Info() => Run(async () => Ok(await _service.GetMyResumeInfo()));

            [Authorize(Roles = "Candidate")]
            [HttpGet("me/download")]
            public Task<IActionResult> Download() => Run(async () =>
            {
                var file = await _service.DownloadMyResume();
                return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
            });

            [Authorize(Roles = "Candidate")]
            [HttpDelete]
            public Task<IActionResult> Delete() => Run(async () =>
                await _service.DeleteMyResume() ? NoContent() : NotFound());

            // Employer/Admin: GET /api/resume/candidates/{candidateId}/download
            [Authorize(Roles = "Employer,Admin")]
            [HttpGet("candidates/{candidateId:guid}/download")]
            public Task<IActionResult> DownloadCandidate(Guid candidateId) => Run(async () =>
            {
                var file = await _service.DownloadCandidateResume(candidateId);
                return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
            });

            // ---------- helpers ----------
            private static ResumeUploadDto ToDto(IFormFile file) => new()
            {
                Content = file.OpenReadStream(),
                FileName = file.FileName,
                Length = file.Length
            };

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


