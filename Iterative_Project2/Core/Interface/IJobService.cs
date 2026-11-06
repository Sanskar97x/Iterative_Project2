using Iterative_Project2.Application.DTOs;
using Iterative_Project2.Application.DTOs.JobDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJob(CreateJobDto createJobDto);
        Task<List<JobListDto>> GetAllJobs();
        Task<JobResponseDto> UpdateJob(Guid id, UpdateJobDto updateJobDto);
        Task<bool> DeleteJob(Guid jobId);
        Task<JobFilterResponseDto> FilterJobs (JobFilterRequestDto jobfilterdto);
    }
}

