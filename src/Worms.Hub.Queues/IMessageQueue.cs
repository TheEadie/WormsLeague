using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Worms.Hub.Queues;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
public interface IMessageQueue<T>
{
    Task<bool> HasPendingMessage();

    Task EnqueueMessage(T message);

    Task<(T?, MessageDetails?, ActivityContext)> DequeueMessage();

    Task DeleteMessage(MessageDetails messageDetails);
}
