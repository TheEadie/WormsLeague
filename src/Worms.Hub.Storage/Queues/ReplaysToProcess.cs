using Azure.Storage.Queues;

namespace Worms.Hub.Storage.Queues;

internal sealed class ReplaysToProcess : IMessageQueue<ReplayToProcessMessage>
{
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string QueueName = "replays-to-process";

    public async Task EnqueueMessage(ReplayToProcessMessage message)
    {
        var queueClient = new QueueClient(ConnectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        _ = await queueClient.SendMessageAsync(message.ReplayId).ConfigureAwait(false);
    }

    public async Task<(ReplayToProcessMessage?, MessageDetails?)> DequeueMessage()
    {
        var queueClient = new QueueClient(ConnectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        var message = await queueClient.ReceiveMessageAsync().ConfigureAwait(false);
        return message is null
            ? (null, null)
            : (new ReplayToProcessMessage(message.Value.MessageText),
                new MessageDetails(message.Value.MessageId, message.Value.PopReceipt));
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var queueClient = new QueueClient(ConnectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt)
            .ConfigureAwait(false);
    }
}
