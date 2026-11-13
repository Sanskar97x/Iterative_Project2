using Iterative_Project2.Application.DTOs.ResumeDto;
using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Services
{
    public class ResumeService : IResumeService
    {
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedExtensions = new() { ".pdf", ".docx" };

        private readonly AppDbContext _db;
        private readonly IFileStorageService _storage;
        private readonly IHttpContextAccessor _http;

        public ResumeService(AppDbContext db, IFileStorageService storage, IHttpContextAccessor http)
        {
            _db = db;
            _storage = storage;
            _http = http;
        }

        // Candidate.Id == User.Id (shared key). Assumes the JWT stores the user id in NameIdentifier.
        private Guid CurrentUserId =>
            Guid.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id : throw new UnauthorizedAccessException();

        private bool IsAdmin => _http.HttpContext?.User.IsInRole("Admin") ?? false;

        private async Task<Candidate> GetMyCandidate() =>
            await _db.Candidates.FirstOrDefaultAsync(c => c.Id == CurrentUserId)
            ?? throw new KeyNotFoundException("Candidate profile not found.");

        // ---------------- Upload / update ----------------
        public async Task<ResumeInfoDto> UploadResume(ResumeUploadDto file)
        {
            var candidate = await GetMyCandidate();
            if (!string.IsNullOrEmpty(candidate.ResumeUrl))
                throw new InvalidOperationException("A resume already exists. Use update to replace it.");

            return await SaveResume(candidate, file);
        }

        public async Task<ResumeInfoDto> UpdateResume(ResumeUploadDto file)
        {
            var candidate = await GetMyCandidate();
            var oldKey = candidate.ResumeUrl;

            var info = await SaveResume(candidate, file);

            if (oldKey != candidate.ResumeUrl) await DeleteIfUnreferenced(oldKey);
            return info;
        }

        private async Task<ResumeInfoDto> SaveResume(Candidate candidate, ResumeUploadDto file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException("Only PDF and DOCX files are allowed.");
            if (file.Length <= 0)
                throw new ArgumentException("The file is empty.");
            if (file.Length > MaxBytes)
                throw new ArgumentException("The file is larger than 5 MB.");

            // Buffer (max 5 MB) so the real content can be checked, not just the file name
            using var ms = new MemoryStream();
            await file.Content.CopyToAsync(ms);
            if (ms.Length > MaxBytes) throw new ArgumentException("The file is larger than 5 MB.");
            if (!HasValidSignature(ms.ToArray(), ext))
                throw new ArgumentException("The file content does not match its extension.");

            ms.Position = 0;
            var key = await _storage.SaveAsync(ms, $"resumes/{candidate.Id:N}", ext);

            try
            {
                var name = Path.GetFileName(file.FileName);
                candidate.ResumeUrl = key;
                candidate.ResumeFileName = name.Length > 255 ? name[^255..] : name;
                candidate.ResumeContentType = ContentTypeFor(ext);
                candidate.ResumeUploadedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            catch
            {
                await _storage.DeleteAsync(key); // don't leave an orphan file behind
                throw;
            }

            return ToInfo(candidate);
        }

        // ---------------- Read / download / delete ----------------
        public async Task<ResumeInfoDto> GetMyResumeInfo() => ToInfo(await GetMyCandidate());

        public async Task<ResumeFileDto> DownloadMyResume() => await OpenResume(await GetMyCandidate());

        public async Task<ResumeFileDto> DownloadCandidateResume(Guid candidateId)
        {
            var me = CurrentUserId;
            var allowed = candidateId == me || IsAdmin ||
                await _db.Applications.AsNoTracking()
                    .AnyAsync(a => a.CandidateId == candidateId && a.Job.Company.OwnerUserId == me);

            if (!allowed) throw new UnauthorizedAccessException("This candidate has not applied to your jobs.");

            var candidate = await _db.Candidates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == candidateId)
                ?? throw new KeyNotFoundException("Candidate not found.");

            return await OpenResume(candidate);
        }

        public async Task<bool> DeleteMyResume()
        {
            var candidate = await GetMyCandidate();
            var key = candidate.ResumeUrl;
            if (string.IsNullOrEmpty(key)) return false;

            candidate.ResumeUrl = string.Empty;
            candidate.ResumeFileName = null;
            candidate.ResumeContentType = null;
            candidate.ResumeUploadedAt = null;
            await _db.SaveChangesAsync();

            await DeleteIfUnreferenced(key);
            return true;
        }

        // ---------------- Helpers ----------------
        private async Task<ResumeFileDto> OpenResume(Candidate candidate)
        {
            if (string.IsNullOrEmpty(candidate.ResumeUrl)) return null;

            var stream = await _storage.OpenReadAsync(candidate.ResumeUrl);
            if (stream is null) return null;

            var ext = Path.GetExtension(candidate.ResumeUrl);
            return new ResumeFileDto
            {
                Content = stream,
                ContentType = candidate.ResumeContentType ?? ContentTypeFor(ext),
                FileName = candidate.ResumeFileName ?? $"resume{ext}"
            };
        }

        // Old files stay if an application still points to them (so employers keep what was submitted)
        private async Task DeleteIfUnreferenced(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            var inUse = await _db.Applications.AnyAsync(a => a.ResumeUrl == key);
            if (!inUse) await _storage.DeleteAsync(key);
        }

        private static ResumeInfoDto ToInfo(Candidate c) => new()
        {
            HasResume = !string.IsNullOrEmpty(c.ResumeUrl),
            FileName = c.ResumeFileName,
            ContentType = c.ResumeContentType,
            UploadedAt = c.ResumeUploadedAt
        };

        private static string ContentTypeFor(string ext) => ext.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        // PDF starts with "%PDF"; DOCX is a zip and starts with "PK\x03\x04"
        private static bool HasValidSignature(byte[] data, string ext)
        {
            if (data.Length < 4) return false;
            return ext switch
            {
                ".pdf" => data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46,
                ".docx" => data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04,
                _ => false
            };
        }
    }
}
 
