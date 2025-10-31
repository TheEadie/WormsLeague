using System.Diagnostics;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Worms.Hub.Queues;

internal abstract class MessageQueue<T>(string queueName, IConfiguration configuration) : IMessageQueue<T>
    where T : class
{
    private readonly TextMapPropagator _propagator = new TraceContextPropagator();

    public async Task<bool> HasPendingMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, queueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var peekedMessage = await queueClient.PeekMessageAsync();
        return peekedMessage.Value is not null;
    }

    public async Task EnqueueMessage(T message)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, queueName);
        _ = await queueClient.CreateIfNotExistsAsync();

        var messageData = new Dictionary<string, string> { ["payload"] = JsonSerializer.Serialize(message) };
        _propagator.Inject(
            new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current),
            messageData,
            (headers, key, value) => headers[key] = value);

        var messageJson = JsonSerializer.Serialize(messageData);
        var base64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson));
        _ = await queueClient.SendMessageAsync(base64String);
    }

    public async Task<(T?, MessageDetails?, ActivityContext?)> DequeueMessage()
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, queueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        var message = await queueClient.ReceiveMessageAsync();

        if (message.Value is null)
        {
            return (null, null, null);
        }

        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(message.Value.MessageText));
        var messageData = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;

        var parentContext = _propagator.Extract(
            default,
            messageData,
            (dict, key) => dict.TryGetValue(key, out var value) ? [value] : []);

        Baggage.Current = parentContext.Baggage;

        var payload = JsonSerializer.Deserialize<T>(messageData["payload"])!;
        return (payload, new MessageDetails(message.Value.MessageId, message.Value.PopReceipt),
            parentContext.ActivityContext);
    }

    public async Task DeleteMessage(MessageDetails messageDetails)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        var queueClient = new QueueClient(connectionString, queueName);
        _ = await queueClient.CreateIfNotExistsAsync();
        _ = await queueClient.DeleteMessageAsync(messageDetails.MessageId, messageDetails.PopReceipt);
    }
}
