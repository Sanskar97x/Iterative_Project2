using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Application.DTOs.JobDto;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Iterative_Project2.Infrastructures.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ---------------------------------------------------------------
        // Create
        // ---------------------------------------------------------------
        public async Task<JobResponseDto> CreateJob(CreateJobDto createJobDto)
        {
            var userId = GetCurrentUserId();

            // The logged-in user must own the company they are posting for
            var company = await _context.Set<Company>()
                .FirstOrDefaultAsync(c => c.Id == createJobDto.CompanyId && c.OwnerUserId == userId);

            if (company == null)
                throw new UnauthorizedAccessException("You do not own this company.");

            ValidateDto(createJobDto);

            var job = new Job
            {
                JobTitle = createJobDto.JobTitle,
                JobDescription = createJobDto.JobDescription,
                JobPosition = createJobDto.JobPosition,
                JobType = createJobDto.JobType,
                JobQualification = createJobDto.JobQualification,
                JobResponsibilities = createJobDto.JobResponsibilities,
                JobRequirements = createJobDto.JobRequirements,
                JobExperience = createJobDto.JobExperience,
                NumberOfVacancies = createJobDto.NumberOfVacancies ?? 1,
                JobMinSalary = createJobDto.JobMinSalary,
                JobMaxSalary = createJobDto.JobMaxSalary,
                SalaryCurrency = createJobDto.SalaryCurrency,
                JobLocation = createJobDto.JobLocation,
                IsRemote = createJobDto.IsRemote,
                JobExpiredAt = createJobDto.JobExpiredAt,
                ApplicationDeadline = createJobDto.ApplicationDeadline,
                CompanyId = company.Id,
                Company = company
            };

            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();

            return MapToResponse(job);
        }

        // ---------------------------------------------------------------
        // Read
        // ---------------------------------------------------------------
        public async Task<List<JobListDto>> GetAllJobs()
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.JobCreatedAt)
                .Select(ToListDto)
                .ToListAsync();
        }

        // ---------------------------------------------------------------
        // Update
        // ---------------------------------------------------------------
        public async Task<JobResponseDto> UpdateJob(Guid id, UpdateJobDto updateJobDto)
        {
            var userId = GetCurrentUserId();

            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.Company.OwnerUserId != userId)
                throw new UnauthorizedAccessException("You do not own this job.");

            ValidateDto(updateJobDto);

            job.JobTitle = updateJobDto.JobTitle;
            job.JobDescription = updateJobDto.JobDescription;
            job.JobPosition = updateJobDto.JobPosition;
            job.JobType = updateJobDto.JobType;
            job.JobQualification = updateJobDto.JobQualification;
            job.JobResponsibilities = updateJobDto.JobResponsibilities;
            job.JobRequirements = updateJobDto.JobRequirements;
            job.JobExperience = updateJobDto.JobExperience;
            job.NumberOfVacancies = updateJobDto.NumberOfVacancies ?? job.NumberOfVacancies;
            job.JobMinSalary = updateJobDto.JobMinSalary;
            job.JobMaxSalary = updateJobDto.JobMaxSalary;
            job.SalaryCurrency = updateJobDto.SalaryCurrency;
            job.JobLocation = updateJobDto.JobLocation;
            job.IsRemote = updateJobDto.IsRemote;
            job.JobExpiredAt = updateJobDto.JobExpiredAt;
            job.ApplicationDeadline = updateJobDto.ApplicationDeadline;
            // CompanyId is intentionally NOT updated: a job can't be moved to another company.

            await _context.SaveChangesAsync();

            return MapToResponse(job);
        }

        // ---------------------------------------------------------------
        // Delete
        // ---------------------------------------------------------------
        public async Task<bool> DeleteJob(Guid jobId)
        {
            var userId = GetCurrentUserId();

            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.Company.OwnerUserId != userId)
                throw new UnauthorizedAccessException("You do not own this job.");

            var hasApplications = await _context.Set<JobApplication>()
                .AnyAsync(a => a.JobId == jobId);

            if (hasApplications)
            {
                // Keep applicants' history: deactivate instead of deleting
                job.IsActive = false;
            }
            else
            {
                _context.Jobs.Remove(job);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ---------------------------------------------------------------
        // Filter + paging
        // ---------------------------------------------------------------
        public async Task<JobFilterResponseDto> FilterJobs(JobFilterRequestDto filter)
        {
            var query = _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(j =>
                    j.JobTitle.Contains(filter.Keyword) ||
                    j.JobDescription.Contains(filter.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                query = query.Where(j => j.JobLocation.Contains(filter.Location));
            }

            if (filter.IsRemote.HasValue)
            {
                query = query.Where(j => j.IsRemote == filter.IsRemote.Value);
            }

            if (filter.JobType.HasValue)
            {
                query = query.Where(j => j.JobType == filter.JobType.Value);
            }

            if (filter.JobPosition.HasValue)
            {
                query = query.Where(j => j.JobPosition == filter.JobPosition.Value);
            }

            if (filter.MinSalary.HasValue)
            {
                // Jobs that can pay at least the requested minimum
                query = query.Where(j => j.JobMaxSalary >= filter.MinSalary.Value);
            }

            if (filter.MaxExperience.HasValue)
            {
                // Jobs that don't require more experience than the candidate has
                query = query.Where(j => j.JobExperience <= filter.MaxExperience.Value);
            }

            if (filter.CompanyId.HasValue)
            {
                query = query.Where(j => j.CompanyId == filter.CompanyId.Value);
            }

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var totalCount = await query.CountAsync();

            var jobs = await query
                .OrderByDescending(j => j.JobCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToListDto)
                .ToListAsync();

            return new JobFilterResponseDto
            {
                Jobs = jobs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        private Guid GetCurrentUserId()
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return userId;
        }

        private static void ValidateDto(CreateJobDto dto)
        {
            if (dto.JobMinSalary > dto.JobMaxSalary)
                throw new ArgumentException("Minimum salary cannot be greater than maximum salary.");

            if (dto.ApplicationDeadline > dto.JobExpiredAt)
                throw new ArgumentException("Application deadline cannot be after the job expiry date.");
        }

        // Single projection used by list and filter (translates to SQL, includes company name)
        private static readonly Expression<Func<Job, JobListDto>> ToListDto = j => new JobListDto
        {
            JobId = j.JobId,
            JobTitle = j.JobTitle,
            JobPosition = j.JobPosition,
            JobType = j.JobType,
            JobExperience = j.JobExperience,
            JobMinSalary = j.JobMinSalary,
            JobMaxSalary = j.JobMaxSalary,
            SalaryCurrency = j.SalaryCurrency,
            JobLocation = j.JobLocation,
            IsRemote = j.IsRemote,
            JobCreatedAt = j.JobCreatedAt,
            ApplicationDeadline = j.ApplicationDeadline,
            CompanyId = j.CompanyId,
            CompanyName = j.Company.CompanyName
        };

        // Requires job.Company to be loaded
        private static JobResponseDto MapToResponse(Job job) => new JobResponseDto
        {
            JobId = job.JobId,
            JobTitle = job.JobTitle,
            JobPosition = job.JobPosition,
            JobType = job.JobType,
            JobExperience = job.JobExperience,
            JobMinSalary = job.JobMinSalary,
            JobMaxSalary = job.JobMaxSalary,
            SalaryCurrency = job.SalaryCurrency,
            JobLocation = job.JobLocation,
            IsRemote = job.IsRemote,
            JobCreatedAt = job.JobCreatedAt,
            ApplicationDeadline = job.ApplicationDeadline,
            CompanyId = job.CompanyId,
            CompanyName = job.Company.CompanyName,
            JobDescription = job.JobDescription,
            JobQualification = job.JobQualification,
            JobResponsibilities = job.JobResponsibilities,
            JobRequirements = job.JobRequirements,
            NumberOfVacancies = job.NumberOfVacancies ?? 1,
            JobExpiredAt = job.JobExpiredAt,
            IsActive = job.IsActive
        };
    }
}