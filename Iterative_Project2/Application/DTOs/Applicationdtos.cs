using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.DTOs
{
     
        // ---------- Input ----------
        public class ApplyJobDto
        {
            [Required] public Guid JobId { get; set; }
            [MaxLength(4000)] public string CoverLetter { get; set; }

            // Link to the resume (Google Drive, portfolio, etc.). Falls back to the candidate's profile resume link.
            [Url, MaxLength(500)] public string ResumeUrl { get; set; }
        }

        public class UpdateApplicationStatusDto
        {
            [Required] public ApplicationStatus Status { get; set; }
        }

        public class ApplicationFilterRequestDto
        {
            public ApplicationStatus? Status { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        // ---------- Shared ----------
        public class PagedResultDto<T>
        {
            public List<T> Items { get; set; } = new();
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }

        // ---------- Candidate side ----------
        public class ApplicationResponseDto
        {
            public Guid ApplicationId { get; set; }
            public Guid JobId { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public Guid CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string JobLocation { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime AppliedDate { get; set; }
            public DateTime StatusUpdatedAt { get; set; }
            public bool JobIsOpen { get; set; }
        }

        public class ApplicationDetailDto : ApplicationResponseDto
        {
            public string CoverLetter { get; set; }
            public string ResumeUrl { get; set; }
        }

        // ---------- Employer side ----------
        public class ApplicantListItemDto
        {
            public Guid ApplicationId { get; set; }
            public Guid CandidateId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; }
            public int YearsOfExperience { get; set; }
            public List<string> Skills { get; set; } = new();
            public string ResumeUrl { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime AppliedDate { get; set; }
            public DateTime StatusUpdatedAt { get; set; }
        }

        public class ApplicantDetailDto
        {
            public Guid ApplicationId { get; set; }
            public Guid JobId { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime AppliedDate { get; set; }
            public DateTime StatusUpdatedAt { get; set; }
            public string CoverLetter { get; set; }
            public string ResumeUrl { get; set; }
            public CandidateProfileDto Candidate { get; set; } = new();
        }

        public class CandidateProfileDto
        {
            public Guid CandidateId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Bio { get; set; }
            public int YearsOfExperience { get; set; }
            public List<SkillItemDto> Skills { get; set; } = new();
            public List<EducationItemDto> Educations { get; set; } = new();
            public List<ExperienceItemDto> Experiences { get; set; } = new();
        }

        public class SkillItemDto
        {
            public string Name { get; set; } = string.Empty;
            public string ProficiencyLevel { get; set; }
        }

        public class EducationItemDto
        {
            public string Institution { get; set; } = string.Empty;
            public string Degree { get; set; } = string.Empty;
            public string FieldOfStudy { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsCurrentlyStudying { get; set; }
        }

        public class ExperienceItemDto
        {
            public string JobTitle { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string EmploymentType { get; set; } = string.Empty;
            public string Location { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsCurrentlyWorking { get; set; }
        }

        public class ApplicationStatusUpdateResultDto
        {
            public Guid ApplicationId { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? StatusUpdatedAt { get; set; }
        }

        public class JobApplicationSummaryDto
        {
            public Guid JobId { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public int TotalApplications { get; set; }
            public Dictionary<string, int> ByStatus { get; set; } = new();
        }
    
}

