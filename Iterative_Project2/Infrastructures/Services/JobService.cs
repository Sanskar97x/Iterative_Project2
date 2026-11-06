using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Application.DTOs.JobDto;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        public JobService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<JobResponseDto>CreateJob(CreateJobDto createJobDto)
        {

            /*var employerId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);*/

            var job = new Job
            {
                JobTitle = createJobDto.JobTitle,
                JobDescription = createJobDto.JobDescription,
                JobPosition = createJobDto.JobDescription,
                JobQualification = createJobDto.JobQualification,
                JobResponsibilities = createJobDto.JobResponsibilities,
                JobRequirements = createJobDto.JobRequirements,
                JobExperience = createJobDto.JobExperience,
                NumberOfVacancies = createJobDto.NumberOfVacancies,
                JobMinSalary = createJobDto.JobMinSalary,
                JobMaxSalary = createJobDto.JobMaxSalary,
                SalaryCurrency = createJobDto.SalaryCurrency,
                JobLocation = createJobDto.JobLocation,
                IsRemote = createJobDto.IsRemote,
                JobExpiredAt = createJobDto.JobExpiredAt,
                ApplicationDeadline = createJobDto.ApplicationDeadline
            };

            //job = await _context.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.JobId == id);
            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobPosition = job.JobPosition,
                JobQualification = job.JobQualification,
                JobResponsibilities = job.JobResponsibilities,
                JobRequirements = job.JobRequirements,
                JobExperience = job.JobExperience,
                NumberOfVacancies = job.NumberOfVacancies,
                JobMinSalary = job.JobMinSalary,
                JobMaxSalary = job.JobMaxSalary,
                SalaryCurrency = job.SalaryCurrency,
                JobLocation = job.JobLocation,
                IsRemote = job.IsRemote,
                JobCreatedAt = job.JobCreatedAt,
                JobExpiredAt = job.JobExpiredAt,
                ApplicationDeadline = job.ApplicationDeadline,
                IsActive = job.IsActive,
            };
        }

        public async Task<List<JobListDto>>GetAllJobs()
        {
            var jobs = await _context.Jobs.Select(j => new JobListDto
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle
            }).ToListAsync();

            return jobs;
        }

        public async Task<JobResponseDto>UpdateJob(Guid id, UpdateJobDto updateJobDto)
        {

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                throw new Exception("Job Not Found");

            job.JobTitle = updateJobDto.JobTitle;
            job.JobDescription = updateJobDto.JobDescription;
            job.JobPosition = updateJobDto.JobPosition;
            job.JobQualification = updateJobDto.JobQualification;
            job.JobResponsibilities = updateJobDto.JobResponsibilities;
            job.JobRequirements = updateJobDto.JobRequirements;
            job.JobExperience = updateJobDto.JobExperience;
            job.NumberOfVacancies = updateJobDto.NumberOfVacancies;
            job.JobMinSalary = updateJobDto.JobMinSalary;
            job.JobMaxSalary = updateJobDto.JobMaxSalary;
            job.SalaryCurrency = updateJobDto.SalaryCurrency;
            job.JobLocation = updateJobDto.JobLocation;
            job.IsRemote = updateJobDto.IsRemote;
            job.JobExpiredAt = updateJobDto.JobExpiredAt;
            job.ApplicationDeadline = updateJobDto.ApplicationDeadline;

            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobPosition = job.JobPosition,
                JobQualification = job.JobQualification,
                JobResponsibilities = job.JobResponsibilities,
                JobRequirements = job.JobRequirements,
                JobExperience = job.JobExperience,
                NumberOfVacancies = job.NumberOfVacancies,
                JobMinSalary = job.JobMinSalary,
                JobMaxSalary = job.JobMaxSalary,
                SalaryCurrency = job.SalaryCurrency,
                JobLocation = job.JobLocation,
                IsRemote = job.IsRemote,
                JobExpiredAt = job.JobExpiredAt,
                ApplicationDeadline = job.ApplicationDeadline
            };
    }

        public async Task<bool>DeleteJob(Guid jobId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null)
            {
                throw new Exception("Job not found");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<JobFilterResponseDto> FilterJobs(JobFilterRequestDto jobfilterdto)
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(jobfilterdto.SearchTerm))
            {
                query = query.Where(j =>
                    j.JobTitle.Contains(jobfilterdto.SearchTerm) ||
                    j.JobDescription.Contains(jobfilterdto.SearchTerm));
            }

            if (!string.IsNullOrEmpty(jobfilterdto.JobPosition))
            {
                query = query.Where(j => j.JobPosition.Contains(jobfilterdto.JobPosition));
            }

            if (jobfilterdto.MinSalary.HasValue)
            {
                query = query.Where(j => j.JobMinSalary >= jobfilterdto.MinSalary.Value);
            }

            if (jobfilterdto.MaxSalary.HasValue)
            {
                query = query.Where(j => j.JobMaxSalary <= jobfilterdto.MaxSalary.Value);
            }

            if (jobfilterdto.Experience.HasValue)
            {
                query = query.Where(j => j.JobExperience >= jobfilterdto.Experience.Value);
            }

            if (jobfilterdto.IsRemote.HasValue)
            {
                query = query.Where(j => j.IsRemote == jobfilterdto.IsRemote.Value);
            }

            var totalCount = await query.CountAsync();

            var jobs = await query
            .Skip((jobfilterdto.PageNumber - 1) * jobfilterdto.PageSize)
            .Take(jobfilterdto.PageSize)
            .Select(j => new JobDto
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle
            }).ToListAsync();


            return new JobFilterResponseDto
            {
                Jobs = jobs
            };
        }
    }
}

