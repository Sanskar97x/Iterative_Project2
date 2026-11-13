using Iterative_Project2.Application.DTOs.CompanyDto;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private const int MaxCompaniesPerOwner = 5;

        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly INotificationPublisher _notifications; // NOTIFY

        public CompanyService(AppDbContext db, IHttpContextAccessor http, INotificationPublisher notifications)
        {
            _db = db;
            _http = http;
            _notifications = notifications; // NOTIFY
        }

        // Assumes the JWT stores the user id in the NameIdentifier claim.
        private Guid? CurrentUserIdOrNull =>
            Guid.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private Guid CurrentUserId => CurrentUserIdOrNull ?? throw new UnauthorizedAccessException();

        private bool IsAdmin => _http.HttpContext?.User.IsInRole("Admin") ?? false;

        // =====================================================================
        // Employer: registration and management
        // =====================================================================
        public async Task<CompanyResponseDto> RegisterCompany(RegisterCompanyDto dto)
        {
            var me = CurrentUserId;

            if (await _db.Companies.CountAsync(c => c.OwnerUserId == me) >= MaxCompaniesPerOwner)
                throw new InvalidOperationException($"You can own at most {MaxCompaniesPerOwner} companies.");

            var name = NormalizeName(dto.CompanyName);
            await EnsureNameAvailable(name, null);

            var company = new Company
            {
                Id = Guid.NewGuid(),
                OwnerUserId = me,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            ApplyFields(company, name, dto);

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            return await LoadResponse(company.Id);
        }

        public async Task<List<CompanyResponseDto>> GetMyCompanies()
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;

            var rows = await _db.Companies.AsNoTracking()
                .Where(c => c.OwnerUserId == me)
                .OrderBy(c => c.CompanyName)
                .Select(c => new { Company = c, ActiveJobs = c.Jobs.Count(j => j.IsActive && j.ApplicationDeadline >= now) })
                .ToListAsync();

            return rows.Select(r => ToResponse(r.Company, r.ActiveJobs)).ToList();
        }

        public async Task<CompanyResponseDto> UpdateCompany(Guid companyId, UpdateCompanyDto dto)
        {
            var company = await GetOwnedCompany(companyId);

            var name = NormalizeName(dto.CompanyName);
            if (!string.Equals(name, company.CompanyName, StringComparison.OrdinalIgnoreCase))
                await EnsureNameAvailable(name, company.Id);

            ApplyFields(company, name, dto);
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await LoadResponse(company.Id);
        }

        public async Task<bool> DeleteCompany(Guid companyId)
        {
            var company = await GetOwnedCompany(companyId);

            if (await _db.Jobs.AnyAsync(j => j.CompanyId == companyId))
                throw new InvalidOperationException("This company has jobs. Deactivate it instead of deleting it.");

            _db.Companies.Remove(company);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<CompanyResponseDto> SetCompanyActive(Guid companyId, bool isActive)
        {
            var company = await GetOwnedCompany(companyId);
            company.IsActive = isActive;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await LoadResponse(company.Id);
        }

        public async Task<CompanyResponseDto> TransferOwnership(Guid companyId, TransferOwnershipDto dto)
        {
            var company = await GetOwnedCompany(companyId);

            var email = dto.NewOwnerEmail.Trim().ToLower();
            var newOwner = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.IsActive)
                ?? throw new KeyNotFoundException("No active user found with that email.");

            if (newOwner.Id == company.OwnerUserId)
                throw new InvalidOperationException("That user already owns this company.");

            // Adjust if your UserRole enum uses a different name for employers
            if (newOwner.Role != UserRole.Employee)
                throw new InvalidOperationException("The new owner must have an employer account.");

            if (await _db.Companies.CountAsync(c => c.OwnerUserId == newOwner.Id) >= MaxCompaniesPerOwner)
                throw new InvalidOperationException("The new owner already owns the maximum number of companies.");

            company.OwnerUserId = newOwner.Id;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await LoadResponse(company.Id);
        }

        public async Task<CompanyStatsDto> GetCompanyStats(Guid companyId)
        {
            await GetOwnedCompany(companyId);

            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var jobs = _db.Jobs.AsNoTracking().Where(j => j.CompanyId == companyId);
            var applications = _db.Applications.AsNoTracking().Where(a => a.Job.CompanyId == companyId);

            var byStatus = await applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var top = await jobs
                .Select(j => new TopJobDto { JobId = j.JobId, JobTitle = j.JobTitle, ApplicationCount = j.Applications.Count })
                .OrderByDescending(j => j.ApplicationCount)
                .Take(5)
                .ToListAsync();

            return new CompanyStatsDto
            {
                TotalJobs = await jobs.CountAsync(),
                ActiveJobs = await jobs.CountAsync(j => j.IsActive && j.ApplicationDeadline >= now),
                TotalApplications = byStatus.Sum(x => x.Count),
                ApplicationsLast7Days = await applications.CountAsync(a => a.AppliedDate >= weekAgo),
                ApplicationsByStatus = Enum.GetValues<ApplicationStatus>()
                    .ToDictionary(s => s.ToString(), s => byStatus.FirstOrDefault(x => x.Status == s)?.Count ?? 0),
                TopJobs = top.Where(t => t.ApplicationCount > 0).ToList()
            };
        }

        // =====================================================================
        // Public
        // =====================================================================
        public async Task<CompanyResponseDto> GetCompanyById(Guid companyId)
        {
            var now = DateTime.UtcNow;
            var row = await _db.Companies.AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => new { Company = c, ActiveJobs = c.Jobs.Count(j => j.IsActive && j.ApplicationDeadline >= now) })
                .FirstOrDefaultAsync();

            return row is null || !CanView(row.Company) ? null : ToResponse(row.Company, row.ActiveJobs);
        }

        public async Task<CompanyFilterResponseDto> SearchCompanies(CompanyFilterRequestDto filter)
        {
            var page = Math.Max(1, filter.Page);
            var size = Math.Clamp(filter.PageSize, 1, 50);
            var now = DateTime.UtcNow;

            var query = _db.Companies.AsNoTracking().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(c => c.CompanyName.Contains(s) || (c.Industry != null && c.Industry.Contains(s)));
            }
            if (!string.IsNullOrWhiteSpace(filter.Industry))
            {
                var industry = filter.Industry.Trim();
                query = query.Where(c => c.Industry != null && c.Industry.Contains(industry));
            }
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var location = filter.Location.Trim();
                query = query.Where(c => c.Location.Contains(location));
            }
            if (filter.VerifiedOnly)
                query = query.Where(c => c.IsVerified);

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(c => c.IsVerified)
                .ThenBy(c => c.CompanyName)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    c.Location,
                    c.Industry,
                    c.CompanySize,
                    c.IsVerified,
                    ActiveJobs = c.Jobs.Count(j => j.IsActive && j.ApplicationDeadline >= now)
                })
                .ToListAsync();

            return new CompanyFilterResponseDto
            {
                Page = page,
                PageSize = size,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Items = rows.Select(r => new CompanyListDto
                {
                    Id = r.Id,
                    CompanyName = r.CompanyName,
                    Location = r.Location,
                    Industry = r.Industry,
                    CompanySize = r.CompanySize?.ToString(),
                    IsVerified = r.IsVerified,
                    ActiveJobCount = r.ActiveJobs
                }).ToList()
            };
        }

        public async Task<List<CompanyJobDto>> GetCompanyJobs(Guid companyId)
        {
            var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId);
            if (company is null || !CanView(company)) return null;

            var now = DateTime.UtcNow;
            var jobs = await _db.Jobs.AsNoTracking()
                .Where(j => j.CompanyId == companyId && j.IsActive && j.ApplicationDeadline >= now)
                .OrderByDescending(j => j.JobCreatedAt)
                .ToListAsync();

            return jobs.Select(j => new CompanyJobDto
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle,
                JobType = j.JobType.ToString(),
                JobPosition = j.JobPosition.ToString(),
                JobLocation = j.JobLocation,
                IsRemote = j.IsRemote,
                JobMinSalary = j.JobMinSalary,
                JobMaxSalary = j.JobMaxSalary,
                SalaryCurrency = j.SalaryCurrency,
                ApplicationDeadline = j.ApplicationDeadline
            }).ToList();
        }

        // =====================================================================
        // Admin
        // =====================================================================
        public async Task<CompanyResponseDto> SetVerified(Guid companyId, bool isVerified)
        {
            if (!IsAdmin) throw new UnauthorizedAccessException();

            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == companyId)
                ?? throw new KeyNotFoundException("Company not found.");

            company.IsVerified = isVerified;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.NotifyCompanyVerification(company.Id, isVerified); // NOTIFY owner

            return await LoadResponse(company.Id);
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        // Owner or Admin only
        private async Task<Company> GetOwnedCompany(Guid companyId)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == companyId)
                ?? throw new KeyNotFoundException("Company not found.");

            if (company.OwnerUserId != CurrentUserId && !IsAdmin)
                throw new UnauthorizedAccessException("You do not own this company.");

            return company;
        }

        // Inactive companies are visible only to their owner and admins
        private bool CanView(Company company)
        {
            if (company.IsActive) return true;
            var me = CurrentUserIdOrNull;
            return me is not null && (company.OwnerUserId == me || IsAdmin);
        }

        private async Task EnsureNameAvailable(string name, Guid? excludeId)
        {
            var lower = name.ToLower();
            var taken = await _db.Companies.AnyAsync(c =>
                c.CompanyName.ToLower() == lower && (excludeId == null || c.Id != excludeId));

            if (taken) throw new InvalidOperationException("A company with this name already exists.");
        }

        private static string NormalizeName(string name)
        {
            var clean = Regex.Replace(name.Trim(), @"\s+", " ");
            if (clean.Length == 0) throw new ArgumentException("Company name is required.");
            return clean;
        }

        private static void ApplyFields(Company company, string name, CompanyInputDto dto)
        {
            if (dto.FoundedYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Founded year cannot be in the future.");

            company.CompanyName = name;
            company.Description = dto.Description?.Trim() ?? string.Empty;
            company.Website = dto.Website?.Trim() ?? string.Empty;
            company.Location = dto.Location?.Trim() ?? string.Empty;
            company.Industry = NullIfBlank(dto.Industry);
            company.CompanySize = dto.CompanySize;
            company.FoundedYear = dto.FoundedYear;
            company.ContactEmail = NullIfBlank(dto.ContactEmail);
            company.ContactPhone = NullIfBlank(dto.ContactPhone);
        }

        private static string NullIfBlank(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private async Task<CompanyResponseDto> LoadResponse(Guid id)
        {
            var now = DateTime.UtcNow;
            var row = await _db.Companies.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new { Company = c, ActiveJobs = c.Jobs.Count(j => j.IsActive && j.ApplicationDeadline >= now) })
                .FirstAsync();
            return ToResponse(row.Company, row.ActiveJobs);
        }

        private static CompanyResponseDto ToResponse(Company c, int activeJobs) => new()
        {
            Id = c.Id,
            CompanyName = c.CompanyName,
            Description = c.Description,
            Website = c.Website,
            Location = c.Location,
            Industry = c.Industry,
            CompanySize = c.CompanySize?.ToString(),
            FoundedYear = (int)c.FoundedYear,
            ContactEmail = c.ContactEmail,
            ContactPhone = c.ContactPhone,
            IsVerified = c.IsVerified,
            IsActive = c.IsActive,
            ActiveJobCount = activeJobs,
            CreatedAt = c.CreatedAt
        };
    }
}