using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Queues;

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
        var messageJson = JsonSerializer.Serialize(message);
        _ = await queueClient.SendMessageAsync(messageJson);
    }

    public async Task<(ReplayToUpdateMessage?, MessageDetails?)> DequeueMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var message = await queueClient.ReceiveMessageAsync();

        if (message.Value is null)
        {
            return (null, null);
        }

        var replayMessage = JsonSerializer.Deserialize<ReplayToUpdateMessage>(message.Value.MessageText);
        return (replayMessage, new MessageDetails(message.Value.MessageId, message.Value.PopReceipt));
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, QueueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt);
    }
}
