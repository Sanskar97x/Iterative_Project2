using Iterative_Project2.Application.DTOs.ResumeDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
        // Errors are signalled with exceptions the controller maps to HTTP codes:
        //   ArgumentException -> 400, InvalidOperationException -> 409,
        //   KeyNotFoundException -> 404, UnauthorizedAccessException -> 403
        public interface IResumeService
        {
            // Candidate
            Task<ResumeInfoDto> UploadResume(ResumeUploadDto file);          // first upload (409 if one exists)
            Task<ResumeInfoDto> UpdateResume(ResumeUploadDto file);          // replace (works even if none yet)
            Task<ResumeInfoDto> GetMyResumeInfo();
            Task<ResumeFileDto> DownloadMyResume();
            Task<bool> DeleteMyResume();

            // Employer/Admin: only for candidates who applied to one of the employer's jobs
            Task<ResumeFileDto> DownloadCandidateResume(Guid candidateId);
        }
    
}
