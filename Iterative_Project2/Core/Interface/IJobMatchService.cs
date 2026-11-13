using Iterative_Project2.Application.DTOs.JobMatchDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public interface IJobMatchService
    {
        // Candidate: best jobs for the logged-in user, highest match first
        Task<List<JobMatchResultDto>> GetRecommendedJobs(int minMatchPercentage = 30, int take = 20);

        // Candidate: how well do I fit this one job (for the job details page)
        Task<JobMatchResultDto> GetMyMatchForJob(Guid jobId);

        // Employer: best candidates for one of their jobs
        Task<List<CandidateMatchResultDto>> GetMatchingCandidates(Guid jobId, int minMatchPercentage = 30, int take = 20);
    }
}