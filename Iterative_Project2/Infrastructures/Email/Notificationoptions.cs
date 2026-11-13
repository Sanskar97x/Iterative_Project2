using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Email
{
    // appsettings.json section "Email"
    public class EmailSettings
    {
        public bool Enabled { get; set; } = false;          // false: emails are only logged (good for development)
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // keep in user-secrets / environment variables
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "JobMatch";
    }

    // appsettings.json section "Notifications"
    public class NotificationSettings
    {
        // In-app notifications are created for every status change;
        // an EMAIL is also sent only when the new status is in this list.
        public string[] EmailOnStatuses { get; set; } = new[] { "Accepted" };

        public bool EmailEmployerOnNewApplication { get; set; } = false;
    }
}