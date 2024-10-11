using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Storage.Queues;

internal sealed class ReplaysToProcess(IConfiguration configuration) : IMessageQueue<ReplayToProcessMessage>
{
    private const string QueueName = "replays-to-process";

    public async Task<bool> HasPendingMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        var peekedMessage = await queueClient.PeekMessageAsync().ConfigureAwait(false);
        return peekedMessage.Value is not null;
    }

    public async Task EnqueueMessage(ReplayToProcessMessage message)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        _ = await queueClient.SendMessageAsync(message.ReplayId).ConfigureAwait(false);
    }

    public async Task<(ReplayToProcessMessage?, MessageDetails?)> DequeueMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        var message = await queueClient.ReceiveMessageAsync().ConfigureAwait(false);
        return message.Value is null
            ? (null, null)
            : (new ReplayToProcessMessage(message.Value.MessageText),
                new MessageDetails(message.Value.MessageId, message.Value.PopReceipt));
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt)
            .ConfigureAwait(false);
    }
}
