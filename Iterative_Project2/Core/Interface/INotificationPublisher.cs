using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    // Used by OTHER services to create notifications (writing side).
    // These methods never throw: a notification problem must not break the main action.
    public interface INotificationPublisher
    {
        Task NotifyAsync(Guid userId, Notificationtype type, string title, string message,
                         Guid? relatedEntityId = null, bool sendEmail = false);

        // Employer is told that a candidate applied
        Task NotifyApplicationReceived(Guid applicationId);

        // Candidate is told the status changed (email only for the statuses listed in settings)
        Task NotifyApplicationStatusChanged(Guid applicationId);

        // Company owner is told the company was verified / unverified
        Task NotifyCompanyVerification(Guid companyId, bool isVerified);
    }
}
