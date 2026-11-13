using System;
using System.IO;

namespace Iterative_Project2.Application.DTOs.ResumeDto
{
    // Framework-neutral upload model (the controller maps IFormFile to this)
    public class ResumeUploadDto
    {
        public Stream Content { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public long Length { get; set; }
    }

    public class ResumeInfoDto
    {
        public bool HasResume { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime? UploadedAt { get; set; }
    }

    public class ResumeFileDto
    {
        public Stream Content { get; set; } = Stream.Null;
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "resume";
    }
}