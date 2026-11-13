using Iterative_Project2.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Iterative_Project2.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/matching")]
    public class JobMatchController : ControllerBase
    {
        private readonly IJobMatchService _service;

        public JobMatchController(IJobMatchService service) => _service = service;

        // NOTE: role names below must match your UserRole enum values (stored as text in the JWT).

        // Candidate: GET /api/matching/recommended-jobs?minMatch=40&take=10
        [Authorize(Roles = "Candidate")]
        [HttpGet("recommended-jobs")]
        public async Task<IActionResult> Recommended([FromQuery] int minMatch = 30, [FromQuery] int take = 20) =>
            Ok(await _service.GetRecommendedJobs(minMatch, take));

        // Candidate: GET /api/matching/jobs/{jobId}/my-match
        [Authorize(Roles = "Candidate")]
        [HttpGet("jobs/{jobId:guid}/my-match")]
        public async Task<IActionResult> MyMatch(Guid jobId)
        {
            var result = await _service.GetMyMatchForJob(jobId);
            return result is null ? NotFound() : Ok(result);
        }

        // Employer: GET /api/matching/jobs/{jobId}/candidates?minMatch=50
        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("jobs/{jobId:guid}/candidates")]
        public async Task<IActionResult> Candidates(Guid jobId, [FromQuery] int minMatch = 30, [FromQuery] int take = 20)
        {
            try
            {
                return Ok(await _service.GetMatchingCandidates(jobId, minMatch, take));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
