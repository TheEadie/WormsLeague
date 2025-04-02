using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Worms.Hub.Queues;

namespace Worms.Hub.ReplayProcessor.Queue;

internal sealed class ReplaysToUpdate(IConfiguration configuration) : IMessageQueue<ReplayToUpdateMessage>
{
    private const string QueueName = "replays-to-update";

    public async Task<bool> HasPendingMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var peekedMessage = await queueClient.PeekMessageAsync();
        return peekedMessage.Value is not null;
    }

    public async Task EnqueueMessage(ReplayToUpdateMessage message)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.SendMessageAsync(message.ReplayFileName);
    }

    public async Task<(ReplayToUpdateMessage?, MessageDetails?)> DequeueMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var message = await queueClient.ReceiveMessageAsync();
        return message.Value is null
            ? (null, null)
            : (new ReplayToUpdateMessage(message.Value.MessageText),
                new MessageDetails(message.Value.MessageId, message.Value.PopReceipt));
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt);
    }
}
