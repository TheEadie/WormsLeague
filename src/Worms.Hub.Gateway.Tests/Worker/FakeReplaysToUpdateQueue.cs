using System.Diagnostics;
using Worms.Hub.Queues;

namespace Worms.Hub.Gateway.Tests.Worker;

internal sealed class FakeReplaysToUpdateQueue : IMessageQueue<ReplayToUpdateMessage>
{
    private readonly List<MessageDetails> _deleted = [];
    private ReplayToUpdateMessage? _message;

    public IReadOnlyList<MessageDetails> Deleted => _deleted;

    public MessageDetails? PendingMessageDetails { get; private set; }

    public Task<bool> HasPendingMessage() => Task.FromResult(_message is not null);

    public Task EnqueueMessage(ReplayToUpdateMessage message)
    {
        _message = message;
        PendingMessageDetails = new MessageDetails(Guid.NewGuid().ToString("N"), "pop-receipt");
        return Task.CompletedTask;
    }

    public Task<(ReplayToUpdateMessage?, MessageDetails?, ActivityContext)> DequeueMessage() =>
        Task.FromResult<(ReplayToUpdateMessage?, MessageDetails?, ActivityContext)>(
            (_message, PendingMessageDetails, default));

    public Task DeleteMessage(MessageDetails messageDetails)
    {
        _deleted.Add(messageDetails);
        return Task.CompletedTask;
    }
}
