using Iterative_Project2.Application.DTOs.JobMatchDto;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Services
{
    public class JobMatchService : IJobMatchService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;

        public JobMatchService(AppDbContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        // Candidate.Id == User.Id (shared primary key), so the logged-in user's id is also the candidate id.
        // Assumes your JWT stores the user id in the NameIdentifier claim.
        private Guid CurrentUserId
        {
            get
            {
                var value = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
            }
        }

        private bool IsAdmin => _http.HttpContext?.User.IsInRole("Admin") ?? false;

        private static string Normalize(string s) => s.Trim().ToLowerInvariant();

        // percent = (required skills the candidate has) / (required skills of the job)
        private static (int percent, List<string> matched, List<string> missing) Score(
            HashSet<string> candidateSkills, IEnumerable<string> requiredSkillNames)
        {
            var required = requiredSkillNames
                .GroupBy(Normalize)
                .Select(g => g.First().Trim())
                .ToList();

            if (required.Count == 0) return (0, new(), new());

            var matched = required.Where(r => candidateSkills.Contains(Normalize(r))).OrderBy(r => r).ToList();
            var missing = required.Where(r => !candidateSkills.Contains(Normalize(r))).OrderBy(r => r).ToList();
            var percent = (int)Math.Round(100.0 * matched.Count / required.Count);
            return (percent, matched, missing);
        }

        private static JobMatchResultDto ToJobResult(Job job, HashSet<string> mySkills)
        {
            var (percent, matched, missing) = Score(mySkills, job.JobSkills.Select(js => js.Name));
            return new JobMatchResultDto
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                CompanyName = job.Company.CompanyName,
                JobLocation = job.JobLocation,
                IsRemote = job.IsRemote,
                MatchPercentage = percent,
                MatchedSkills = matched,
                MissingSkills = missing
            };
        }

        private async Task<HashSet<string>> GetMySkills()
        {
            var myId = CurrentUserId;
            var names = await _db.Skills.AsNoTracking()
                .Where(s => s.CandidateId == myId)
                .Select(s => s.Name)
                .ToListAsync();
            return names.Select(Normalize).ToHashSet();
        }

        // ---------- Candidate: recommended jobs ----------
        public async Task<List<JobMatchResultDto>> GetRecommendedJobs(int minMatchPercentage = 30, int take = 20)
        {
            var myId = CurrentUserId;
            var mySkills = await GetMySkills();
            if (mySkills.Count == 0) return new();

            var mySkillList = mySkills.ToList();
            var now = DateTime.UtcNow;

            var appliedJobIds = await _db.Applications.AsNoTracking()
                .Where(a => a.CandidateId == myId)
                .Select(a => a.JobId)
                .ToListAsync();

            // Only open jobs the candidate hasn't applied to and that share at least one skill
            var jobs = await _db.Jobs.AsNoTracking()
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                .Where(j => j.IsActive
                         && j.ApplicationDeadline >= now
                         && !appliedJobIds.Contains(j.JobId)
                         && j.JobSkills.Any(js => mySkillList.Contains(js.Name.ToLower())))
                .ToListAsync();

            return jobs
                .Select(j => ToJobResult(j, mySkills))
                .Where(r => r.MatchPercentage >= minMatchPercentage)
                .OrderByDescending(r => r.MatchPercentage)
                .ThenBy(r => r.JobTitle)
                .Take(take)
                .ToList();
        }

        // ---------- Candidate: match for one job ----------
        public async Task<JobMatchResultDto> GetMyMatchForJob(Guid jobId)
        {
            var job = await _db.Jobs.AsNoTracking()
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job is null) return null;

            return ToJobResult(job, await GetMySkills());
        }

        // ---------- Employer: best candidates for one of their jobs ----------
        public async Task<List<CandidateMatchResultDto>> GetMatchingCandidates(
            Guid jobId, int minMatchPercentage = 30, int take = 20)
        {
            var job = await _db.Jobs.AsNoTracking()
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job is null) return new();

            // Employers can only see matches for jobs of companies they own
            if (job.Company.OwnerUserId != CurrentUserId && !IsAdmin)
                throw new UnauthorizedAccessException("You do not own this job.");

            var requiredNames = job.JobSkills.Select(js => js.Name).ToList();
            var requiredNormalized = requiredNames.Select(Normalize).Distinct().ToList();
            if (requiredNormalized.Count == 0) return new();

            // Only active candidates who have at least one of the required skills
            var candidates = await _db.Candidates.AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Skills)
                .Where(c => c.User.IsActive
                         && c.Skills.Any(s => requiredNormalized.Contains(s.Name.ToLower())))
                .ToListAsync();

            return candidates
                .Select(c =>
                {
                    var theirSkills = c.Skills.Select(s => Normalize(s.Name)).ToHashSet();
                    var (percent, matched, missing) = Score(theirSkills, requiredNames);
                    return new CandidateMatchResultDto
                    {
                        CandidateId = c.Id,
                        FullName = c.User.Fullname,
                        Email = c.User.Email,
                        ResumeUrl = c.ResumeUrl,
                        YearsOfExperience = c.User.YearsOfExperience,
                        MatchPercentage = percent,
                        MatchedSkills = matched,
                        MissingSkills = missing
                    };
                })
                .Where(r => r.MatchPercentage >= minMatchPercentage)
                .OrderByDescending(r => r.MatchPercentage)
                .ThenByDescending(r => r.YearsOfExperience)
                .Take(take)
                .ToList();
        }
    }
}