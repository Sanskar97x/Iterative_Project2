using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public record EmailMessage(string To, string Subject, string HtmlBody);

    // Emails are queued and sent by a background service, so requests never wait on the mail server
    public interface IEmailQueue
    {
        ValueTask EnqueueAsync(EmailMessage message);
    }
}
