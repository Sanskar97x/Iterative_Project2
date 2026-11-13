using Iterative_Project2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
        // Errors are signalled with exceptions the controller maps to HTTP codes:
        //   ArgumentException -> 400, InvalidOperationException -> 409,
        //   KeyNotFoundException -> 404, UnauthorizedAccessException -> 403
        public interface IApplicationService
        {
            // ---- Candidate ----
            Task<ApplicationDetailDto> ApplyToJob(ApplyJobDto dto);
            Task<PagedResultDto<ApplicationResponseDto>> GetMyApplications(ApplicationFilterRequestDto filter);
            Task<ApplicationDetailDto> GetMyApplication(Guid applicationId);
            Task<bool> HasApplied(Guid jobId);
            Task<bool> WithdrawApplication(Guid applicationId);              // only while Pending

            // ---- Employer / Admin (must own the job's company) ----
            Task<PagedResultDto<ApplicantListItemDto>> GetJobApplicants(Guid jobId, ApplicationFilterRequestDto filter);
            Task<JobApplicationSummaryDto> GetJobApplicationSummary(Guid jobId);
            Task<ApplicantDetailDto> GetApplicantDetail(Guid applicationId);
            Task<ApplicationStatusUpdateResultDto> UpdateApplicationStatus(Guid applicationId, UpdateApplicationStatusDto dto);
        }
    
}

