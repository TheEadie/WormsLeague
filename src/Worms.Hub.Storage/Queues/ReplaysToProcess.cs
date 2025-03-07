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
        _ = await queueClient.CreateIfNotExistsAsync();
        var peekedMessage = await queueClient.PeekMessageAsync();
        return peekedMessage.Value is not null;
    }

    public async Task EnqueueMessage(ReplayToProcessMessage message)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.SendMessageAsync(message.ReplayId);
    }

    public async Task<(ReplayToProcessMessage?, MessageDetails?)> DequeueMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var message = await queueClient.ReceiveMessageAsync();
        return message.Value is null
            ? (null, null)
            : (new ReplayToProcessMessage(message.Value.MessageText),
                new MessageDetails(message.Value.MessageId, message.Value.PopReceipt));
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt)
            ;
    }
}
