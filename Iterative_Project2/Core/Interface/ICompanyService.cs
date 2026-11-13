using Iterative_Project2.Application.DTOs.CompanyDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    // Errors are signalled with exceptions the controller maps to HTTP codes:
    //   ArgumentException -> 400, InvalidOperationException -> 409,
    //   KeyNotFoundException -> 404, UnauthorizedAccessException -> 403
    public interface ICompanyService
    {
        // ---- Employer: registration and management ----
        Task<CompanyResponseDto> RegisterCompany(RegisterCompanyDto dto);
        Task<List<CompanyResponseDto>> GetMyCompanies();
        Task<CompanyResponseDto> UpdateCompany(Guid companyId, UpdateCompanyDto dto);
        Task<bool> DeleteCompany(Guid companyId);                          // only when it has no jobs
        Task<CompanyResponseDto> SetCompanyActive(Guid companyId, bool isActive);
        Task<CompanyResponseDto> TransferOwnership(Guid companyId, TransferOwnershipDto dto);
        Task<CompanyStatsDto> GetCompanyStats(Guid companyId);

        // ---- Public ----
        Task<CompanyResponseDto> GetCompanyById(Guid companyId);
        Task<CompanyFilterResponseDto> SearchCompanies(CompanyFilterRequestDto filter);
        Task<List<CompanyJobDto>> GetCompanyJobs(Guid companyId);         // open jobs only

        // ---- Admin ----
        Task<CompanyResponseDto> SetVerified(Guid companyId, bool isVerified);
    }
}