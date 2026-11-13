using Iterative_Project2.Core.Entities;
using Iterative_Project2.Core.Enums;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Iterative_Project2.Infrastructures.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private const string AppName = "JobMatch";

        private readonly AppDbContext _db;
        private readonly IEmailQueue _email;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationPublisher> _logger;

        public NotificationPublisher(
            AppDbContext db,
            IEmailQueue email,
            IOptions<NotificationSettings> settings,
            ILogger<NotificationPublisher> logger)
        {
            _db = db;
            _email = email;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task NotifyAsync(Guid userId, Notificationtype type, string title, string message,
                                      Guid? relatedEntityId = null, bool sendEmail = false)
        {
            try
            {
                var cleanTitle = Clean(title, 150);
                var cleanMessage = Clean(message, 1000);

                _db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = type,
                    Title = cleanTitle,
                    Message = cleanMessage,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                if (sendEmail)
                {
                    var email = await _db.Users.AsNoTracking()
                        .Where(u => u.Id == userId && u.IsActive)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(email))
                        await _email.EnqueueAsync(new EmailMessage(email, cleanTitle, BuildHtml(cleanTitle, cleanMessage)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create a {Type} notification for user {UserId}", type, userId);
            }
        }

        public async Task NotifyApplicationReceived(Guid applicationId)
        {
            try
            {
                var info = await _db.Applications.AsNoTracking()
                    .Where(a => a.Id == applicationId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Job.JobTitle,
                        OwnerId = a.Job.Company.OwnerUserId,
                        CandidateName = a.Candidate.User.Fullname
                    })
                    .FirstOrDefaultAsync();

                if (info is null) return;

                await NotifyAsync(
                    info.OwnerId,
                    Notificationtype.ApplicationReceived,
                    "New application",
                    $"{info.CandidateName} applied for {info.JobTitle}.",
                    info.Id,
                    _settings.EmailEmployerOnNewApplication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not notify about application {ApplicationId}", applicationId);
            }
        }

        public async Task NotifyApplicationStatusChanged(Guid applicationId)
        {
            try
            {
                var info = await _db.Applications.AsNoTracking()
                    .Where(a => a.Id == applicationId)
                    .Select(a => new
                    {
                        a.Id,
                        a.CandidateId,          // Candidate.Id == User.Id
                        a.Status,
                        a.Job.JobTitle,
                        a.Job.Company.CompanyName
                    })
                    .FirstOrDefaultAsync();

                if (info is null) return;

                var status = info.Status.ToString();
                var sendEmail = _settings.EmailOnStatuses
                    .Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));

                await NotifyAsync(
                    info.CandidateId,
                    Notificationtype.ApplicationStatusChanged,
                    "Application update",
                    $"Your application for {info.JobTitle} at {info.CompanyName} is now: {status}.",
                    info.Id,
                    sendEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not notify about status change of application {ApplicationId}", applicationId);
            }
        }

        public async Task NotifyCompanyVerification(Guid companyId, bool isVerified)
        {
            try
            {
                var info = await _db.Companies.AsNoTracking()
                    .Where(c => c.Id == companyId)
                    .Select(c => new { c.Id, c.CompanyName, c.OwnerUserId })
                    .FirstOrDefaultAsync();

                if (info is null) return;

                await NotifyAsync(
                    info.OwnerUserId,
                    Notificationtype.CompanyVerification,
                    isVerified ? "Company verified" : "Company verification removed",
                    isVerified
                        ? $"{info.CompanyName} is now a verified company."
                        : $"The verified status of {info.CompanyName} was removed.",
                    info.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not notify about verification of company {CompanyId}", companyId);
            }
        }

        // ---------- helpers ----------
        // Single line, trimmed, length-limited (also keeps line breaks out of email subjects)
        private static string Clean(string value, int maxLength)
        {
            var text = value.Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }

        // Values are HTML-encoded because job titles and names are user-provided
        private static string BuildHtml(string title, string message) =>
            "<div style=\"font-family:Arial,sans-serif;max-width:560px;margin:auto\">" +
            $"<h2>{WebUtility.HtmlEncode(title)}</h2>" +
            $"<p>{WebUtility.HtmlEncode(message)}</p>" +
            $"<p style=\"color:#888;font-size:12px\">You received this email because of activity on your {AppName} account.</p>" +
            "</div>";
    }
}