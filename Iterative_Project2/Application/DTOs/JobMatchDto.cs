using System;using System.Collections.Generic;

namespace Iterative_Project2.Application.DTOs.JobMatchDto
{
        public class JobMatchResultDto
        {
            public Guid JobId { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string JobLocation { get; set; } = string.Empty;
            public bool IsRemote { get; set; }
            public int MatchPercentage { get; set; }                  // 0-100
            public List<string> MatchedSkills { get; set; } = new();
            public List<string> MissingSkills { get; set; } = new();
        }

        public class CandidateMatchResultDto
        {
            public Guid CandidateId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string ResumeUrl { get; set; }
            public int YearsOfExperience { get; set; }
            public int MatchPercentage { get; set; }
            public List<string> MatchedSkills { get; set; } = new();
            public List<string> MissingSkills { get; set; } = new();
        }
 }

