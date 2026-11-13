using Iterative_Project2.Core.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Email
{
    // Reads queued emails and sends them over SMTP (3 attempts each)
    public class EmailSenderBackgroundService : BackgroundService
    {
        private const int MaxAttempts = 3;

        private readonly EmailQueue _queue;
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailSenderBackgroundService> _logger;

        public EmailSenderBackgroundService(
            EmailQueue queue,
            IOptions<EmailSettings> settings,
            ILogger<EmailSenderBackgroundService> logger)
        {
            _queue = queue;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
                {
                    if (!_settings.Enabled)
                    {
                        _logger.LogInformation("Email is disabled. Skipped '{Subject}' to {To}", message.Subject, message.To);
                        continue;
                    }

                    await SendWithRetry(message, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // application is shutting down
            }
        }

        private async Task SendWithRetry(EmailMessage message, CancellationToken ct)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var client = new SmtpClient(_settings.Host, _settings.Port)
                    {
                        EnableSsl = _settings.EnableSsl,
                        Credentials = new NetworkCredential(_settings.Username, _settings.Password)
                    };
                    using var mail = new MailMessage
                    {
                        From = new MailAddress(_settings.FromAddress, _settings.FromName),
                        Subject = message.Subject,
                        Body = message.HtmlBody,
                        IsBodyHtml = true
                    };
                    mail.To.Add(message.To);

                    await client.SendMailAsync(mail, ct);
                    return;
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    if (attempt == MaxAttempts)
                    {
                        _logger.LogError(ex, "Giving up sending '{Subject}' to {To} after {Attempts} attempts",
                            message.Subject, message.To, MaxAttempts);
                        return;
                    }

                    _logger.LogWarning(ex, "Sending '{Subject}' failed (attempt {Attempt}), retrying", message.Subject, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
                }
            }
        }
    }
}