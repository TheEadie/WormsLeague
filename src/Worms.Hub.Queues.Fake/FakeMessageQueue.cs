using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace Worms.Hub.Queues.Fake;

[PublicAPI]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
public sealed class FakeMessageQueue<T> : IMessageQueue<T>
    where T : class
{
    private readonly List<(T Message, MessageDetails Details)> _pending = [];
    private readonly List<T> _deleted = [];
    private int _nextId = 1;

    public IReadOnlyList<T> Pending => _pending.ConvertAll(m => m.Message);

    public IReadOnlyList<T> Deleted => _deleted;

    public Task<bool> HasPendingMessage() => Task.FromResult(_pending.Count > 0);

    public Task EnqueueMessage(T message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var messageId = _nextId.ToString(CultureInfo.InvariantCulture);
        _nextId++;
        _pending.Add((message, new MessageDetails(messageId, $"pop-receipt-{messageId}")));
        return Task.CompletedTask;
    }

    public Task<(T?, MessageDetails?, ActivityContext)> DequeueMessage()
    {
        if (_pending.Count == 0)
        {
            return Task.FromResult<(T?, MessageDetails?, ActivityContext)>((null, null, default));
        }

        var (message, details) = _pending[0];
        return Task.FromResult<(T?, MessageDetails?, ActivityContext)>((message, details, default));
    }

    public Task DeleteMessage(MessageDetails messageDetails)
    {
        ArgumentNullException.ThrowIfNull(messageDetails);
        var index = _pending.FindIndex(m => m.Details == messageDetails);
        if (index < 0)
        {
            throw new InvalidOperationException(
                $"No pending message with id '{messageDetails.MessageId}' and a matching pop receipt.");
        }

        _deleted.Add(_pending[index].Message);
        _pending.RemoveAt(index);
        return Task.CompletedTask;
    }
}
