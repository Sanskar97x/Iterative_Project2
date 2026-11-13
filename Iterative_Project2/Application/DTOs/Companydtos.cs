using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Iterative_Project2.Application.DTOs.CompanyDto
{
    // ---------- Input ----------
    public class CompanyInputDto
    {
        [Required, MaxLength(150)] public string CompanyName { get; set; } = string.Empty;
        [MaxLength(2000)] public string Description { get; set; }
        [Url, MaxLength(255)] public string Website { get; set; }
        [MaxLength(150)] public string Location { get; set; }
        [MaxLength(100)] public string Industry { get; set; }
        public CompanySize? CompanySize { get; set; }
        [Range(1800, 2100)] public int FoundedYear { get; set; }
        [EmailAddress, MaxLength(150)] public string ContactEmail { get; set; }
        [Phone, MaxLength(30)] public string ContactPhone { get; set; }
    }

    public class RegisterCompanyDto : CompanyInputDto { }

    public class UpdateCompanyDto : CompanyInputDto { }

    public class TransferOwnershipDto
    {
        [Required, EmailAddress] public string NewOwnerEmail { get; set; } = string.Empty;
    }

    public class CompanyFilterRequestDto
    {
        public string Search { get; set; }          // name or industry
        public string Industry { get; set; }
        public string Location { get; set; }
        public bool VerifiedOnly { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    // ---------- Output ----------
    public class CompanyResponseDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public string Industry { get; set; }
        public string CompanySize { get; set; }
        public int FoundedYear { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public int ActiveJobCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CompanyListDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; }
        public string Industry { get; set; }
        public string CompanySize { get; set; }
        public bool IsVerified { get; set; }
        public int ActiveJobCount { get; set; }
    }

    public class CompanyFilterResponseDto
    {
        public List<CompanyListDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CompanyJobDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string JobPosition { get; set; } = string.Empty;
        public string JobLocation { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public int JobMinSalary { get; set; }
        public int JobMaxSalary { get; set; }
        public string SalaryCurrency { get; set; } = string.Empty;
        public DateTime ApplicationDeadline { get; set; }
    }

    // Employer dashboard numbers
    public class CompanyStatsDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int ApplicationsLast7Days { get; set; }
        public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
        public List<TopJobDto> TopJobs { get; set; } = new();   // most applied-to jobs
    }

    public class TopJobDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int ApplicationCount { get; set; }
    }
}