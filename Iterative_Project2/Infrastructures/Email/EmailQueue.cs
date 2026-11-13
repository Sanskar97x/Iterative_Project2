using Iterative_Project2.Core.Interface;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Iterative_Project2.Infrastructures.Email
{
    // In-memory queue. If the app restarts, queued emails that were not sent yet are lost
    // (fine for this project; use a database outbox or a message broker for production).
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
            new BoundedChannelOptions(500) { FullMode = BoundedChannelFullMode.DropOldest });

        public ValueTask EnqueueAsync(EmailMessage message) => _channel.Writer.WriteAsync(message);

        ValueTask IEmailQueue.EnqueueAsync(EmailMessage message)
        {
            throw new System.NotImplementedException();
        }

        public ChannelReader<EmailMessage> Reader => _channel.Reader;
    }
}