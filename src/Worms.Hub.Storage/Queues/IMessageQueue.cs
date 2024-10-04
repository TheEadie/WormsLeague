using System.Diagnostics.CodeAnalysis;

namespace Worms.Hub.Storage.Queues;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
public interface IMessageQueue<T>
{
    Task<int> GetLength();

    Task EnqueueMessage(T message);

    Task<(T?, MessageDetails?)> DequeueMessage();

    Task DeleteMessage(MessageDetails messageDetails);
}
