using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Enums;
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
    public class ApplicationService : IApplicationService
    {
        // true: a resume link is needed either on the application or on the candidate's profile
        private const bool RequireResume = true;

        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly INotificationPublisher _notifications; // NOTIFY

        public ApplicationService(AppDbContext db, IHttpContextAccessor http, INotificationPublisher notifications)
        {
            _db = db;
            _http = http;
            _notifications = notifications; // NOTIFY
        }

        // Candidate.Id == User.Id (shared key). Assumes the JWT stores the user id in NameIdentifier.
        private Guid CurrentUserId =>
            Guid.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id : throw new UnauthorizedAccessException();

        private bool IsAdmin => _http.HttpContext?.User.IsInRole("Admin") ?? false;

        // =====================================================================
        // Candidate
        // =====================================================================
        public async Task<ApplicationDetailDto> ApplyToJob(ApplyJobDto dto)
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;

            var candidate = await _db.Candidates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == me)
                ?? throw new KeyNotFoundException("Candidate profile not found.");

            var job = await _db.Jobs.AsNoTracking().Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == dto.JobId)
                ?? throw new KeyNotFoundException("Job not found.");

            if (!job.IsActive || job.ApplicationDeadline < now || !job.Company.IsActive)
                throw new InvalidOperationException("This job is no longer accepting applications.");

            if (await _db.Applications.AnyAsync(a => a.CandidateId == me && a.JobId == job.JobId))
                throw new InvalidOperationException("You have already applied to this job.");

            var resume = NullIfBlank(dto.ResumeUrl) ?? NullIfBlank(candidate.ResumeUrl);
            if (resume is null && RequireResume)
                throw new ArgumentException("Add a resume link to your profile or to this application.");
            if (resume is not null && !IsHttpUrl(resume))
                throw new ArgumentException("The resume link must be a valid http or https URL.");

            var application = new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = me,
                JobId = job.JobId,
                CoverLetter = NullIfBlank(dto.CoverLetter) ?? string.Empty,
                ResumeUrl = resume ?? string.Empty,
                Status = ApplicationStatus.Pending,
                AppliedDate = now
            };

            _db.Applications.Add(application);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two requests at once: the unique (CandidateId, JobId) index stops the second one
                _db.Entry(application).State = EntityState.Detached;
                if (await _db.Applications.AnyAsync(a => a.CandidateId == me && a.JobId == job.JobId))
                    throw new InvalidOperationException("You have already applied to this job.");
                throw;
            }

            await _notifications.NotifyApplicationReceived(application.Id); // NOTIFY employer

            return (await GetMyApplication(application.Id))!;
        }

        public async Task<PagedResultDto<ApplicationResponseDto>> GetMyApplications(ApplicationFilterRequestDto filter)
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;
            var (page, size) = Clamp(filter);

            var query = _db.Applications.AsNoTracking().Where(a => a.CandidateId == me);
            if (filter.Status is { } status) query = query.Where(a => a.Status == status);

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(a => a.AppliedDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new
                {
                    a.Id,
                    a.JobId,
                    a.Job.JobTitle,
                    a.Job.CompanyId,
                    a.Job.Company.CompanyName,
                    a.Job.JobLocation,
                    a.Status,
                    a.AppliedDate,
                    a.StatusUpdatedAt,
                    IsOpen = a.Job.IsActive && a.Job.ApplicationDeadline >= now
                })
                .ToListAsync();

            return Paged(rows.Select(r => new ApplicationResponseDto
            {
                ApplicationId = r.Id,
                JobId = r.JobId,
                JobTitle = r.JobTitle,
                CompanyId = r.CompanyId,
                CompanyName = r.CompanyName,
                JobLocation = r.JobLocation,
                Status = r.Status.ToString(),
                AppliedDate = r.AppliedDate,
                StatusUpdatedAt = (DateTime)r.StatusUpdatedAt,
                JobIsOpen = r.IsOpen
            }).ToList(), page, size, total);
        }

        public async Task<ApplicationDetailDto> GetMyApplication(Guid applicationId)
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;

            var r = await _db.Applications.AsNoTracking()
                .Where(a => a.Id == applicationId && a.CandidateId == me)
                .Select(a => new
                {
                    a.Id,
                    a.JobId,
                    a.Job.JobTitle,
                    a.Job.CompanyId,
                    a.Job.Company.CompanyName,
                    a.Job.JobLocation,
                    a.Status,
                    a.AppliedDate,
                    a.StatusUpdatedAt,
                    a.CoverLetter,
                    a.ResumeUrl,
                    IsOpen = a.Job.IsActive && a.Job.ApplicationDeadline >= now
                })
                .FirstOrDefaultAsync();

            return r is null
                ? null
                : new ApplicationDetailDto
                {
                    ApplicationId = r.Id,
                    JobId = r.JobId,
                    JobTitle = r.JobTitle,
                    CompanyId = r.CompanyId,
                    CompanyName = r.CompanyName,
                    JobLocation = r.JobLocation,
                    Status = r.Status.ToString(),
                    AppliedDate = r.AppliedDate,
                    StatusUpdatedAt = (DateTime)r.StatusUpdatedAt,
                    JobIsOpen = r.IsOpen,
                    CoverLetter = r.CoverLetter,
                    ResumeUrl = r.ResumeUrl
                };
        }

        public async Task<bool> HasApplied(Guid jobId)
        {
            var me = CurrentUserId;
            return await _db.Applications.AsNoTracking().AnyAsync(a => a.CandidateId == me && a.JobId == jobId);
        }

        public async Task<bool> WithdrawApplication(Guid applicationId)
        {
            var me = CurrentUserId;
            var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId && a.CandidateId == me);
            if (application is null) return false;

            if (application.Status != ApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be withdrawn.");

            _db.Applications.Remove(application);
            await _db.SaveChangesAsync();
            return true;
        }

        // =====================================================================
        // Employer / Admin
        // =====================================================================
        public async Task<PagedResultDto<ApplicantListItemDto>> GetJobApplicants(Guid jobId, ApplicationFilterRequestDto filter)
        {
            await GetManagedJob(jobId);
            var (page, size) = Clamp(filter);

            var query = _db.Applications.AsNoTracking().Where(a => a.JobId == jobId);
            if (filter.Status is { } status) query = query.Where(a => a.Status == status);

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(a => a.AppliedDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new
                {
                    a.Id,
                    a.CandidateId,
                    a.Candidate.User.Fullname,
                    a.Candidate.User.Email,
                    a.Candidate.User.PhoneNumber,
                    a.Candidate.User.YearsOfExperience,
                    Skills = a.Candidate.Skills.Select(s => s.Name).ToList(),
                    a.ResumeUrl,
                    a.Status,
                    a.AppliedDate,
                    a.StatusUpdatedAt
                })
                .ToListAsync();

            return Paged(rows.Select(r => new ApplicantListItemDto
            {
                ApplicationId = r.Id,
                CandidateId = r.CandidateId,
                FullName = r.Fullname,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                YearsOfExperience = r.YearsOfExperience,
                Skills = r.Skills,
                ResumeUrl = r.ResumeUrl,
                Status = r.Status.ToString(),
                AppliedDate = r.AppliedDate,
                StatusUpdatedAt = (DateTime)r.StatusUpdatedAt
            }).ToList(), page, size, total);
        }

        public async Task<JobApplicationSummaryDto> GetJobApplicationSummary(Guid jobId)
        {
            var job = await GetManagedJob(jobId);

            var counts = await _db.Applications.AsNoTracking()
                .Where(a => a.JobId == jobId)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new JobApplicationSummaryDto
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                TotalApplications = counts.Sum(c => c.Count),
                ByStatus = Enum.GetValues<ApplicationStatus>()
                    .ToDictionary(s => s.ToString(), s => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0)
            };
        }

        public async Task<ApplicantDetailDto> GetApplicantDetail(Guid applicationId)
        {
            var application = await _db.Applications.AsNoTracking().AsSplitQuery()
                .Include(a => a.Job).ThenInclude(j => j.Company)
                .Include(a => a.Candidate).ThenInclude(c => c.User)
                .Include(a => a.Candidate).ThenInclude(c => c.Skills)
                .Include(a => a.Candidate).ThenInclude(c => c.Educations)
                .Include(a => a.Candidate).ThenInclude(c => c.Experiences)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application is null) return null;
            EnsureCanManage(application.Job);

            var c = application.Candidate;
            return new ApplicantDetailDto
            {
                ApplicationId = application.Id,
                JobId = application.JobId,
                JobTitle = application.Job.JobTitle,
                Status = application.Status.ToString(),
                AppliedDate = application.AppliedDate,
                StatusUpdatedAt = (DateTime)application.StatusUpdatedAt,
                CoverLetter = application.CoverLetter,
                ResumeUrl = application.ResumeUrl,
                Candidate = new CandidateProfileDto
                {
                    CandidateId = c.Id,
                    FullName = c.User.Fullname,
                    Email = c.User.Email,
                    PhoneNumber = c.User.PhoneNumber,
                    Address = c.User.Address,
                    Bio = c.User.Bio,
                    YearsOfExperience = c.User.YearsOfExperience,
                    Skills = c.Skills.OrderBy(s => s.Name)
                        .Select(s => new SkillItemDto { Name = s.Name, ProficiencyLevel = s.ProficiencyLevel }).ToList(),
                    Educations = c.Educations.OrderByDescending(e => e.StartDate)
                        .Select(e => new EducationItemDto
                        {
                            Institution = e.Institution,
                            Degree = e.Degree,
                            FieldOfStudy = e.FieldOfStudy,
                            StartDate = e.StartDate,
                            EndDate = (DateTime)e.EndDate,
                            IsCurrentlyStudying = e.IsCurrentlyStudying
                        }).ToList(),
                    Experiences = c.Experiences.OrderByDescending(x => x.StartDate)
                        .Select(x => new ExperienceItemDto
                        {
                            JobTitle = x.JobTitle,
                            CompanyName = x.CompanyName,
                            EmploymentType = x.EmploymentType.ToString(),
                            Location = x.Location,
                            Description = x.Description,
                            StartDate = x.StartDate,
                            EndDate = (DateTime)x.EndDate,
                            IsCurrentlyWorking = x.IsCurrentlyWorking
                        }).ToList()
                }
            };
        }

        public async Task<ApplicationStatusUpdateResultDto> UpdateApplicationStatus(
            Guid applicationId, UpdateApplicationStatusDto dto)
        {
            if (!Enum.IsDefined(dto.Status)) throw new ArgumentException("Invalid status.");
            if (dto.Status == ApplicationStatus.Pending)
                throw new ArgumentException("An application cannot be moved back to Pending.");

            var application = await _db.Applications
                .Include(a => a.Job).ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new KeyNotFoundException("Application not found.");

            EnsureCanManage(application.Job);

            if (application.Status == dto.Status)
                throw new InvalidOperationException("The application already has this status.");

            application.Status = dto.Status;
            application.StatusUpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.NotifyApplicationStatusChanged(application.Id); // NOTIFY candidate

            return new ApplicationStatusUpdateResultDto
            {
                ApplicationId = application.Id,
                Status = application.Status.ToString(),
                StatusUpdatedAt = application.StatusUpdatedAt
            };
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        // Loads the job and checks the caller owns its company (or is an Admin)
        private async Task<Job> GetManagedJob(Guid jobId)
        {
            var job = await _db.Jobs.AsNoTracking().Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == jobId)
                ?? throw new KeyNotFoundException("Job not found.");

            EnsureCanManage(job);
            return job;
        }

        private void EnsureCanManage(Job job)
        {
            if (job.Company.OwnerUserId != CurrentUserId && !IsAdmin)
                throw new UnauthorizedAccessException("You do not own this job.");
        }

        private static (int page, int size) Clamp(ApplicationFilterRequestDto f) =>
            (Math.Max(1, f.Page), Math.Clamp(f.PageSize, 1, 50));

        private static PagedResultDto<T> Paged<T>(List<T> items, int page, int size, int total) => new()
        {
            Items = items,
            Page = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };

        private static string NullIfBlank(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsHttpUrl(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}