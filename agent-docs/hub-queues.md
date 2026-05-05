# Hub Queues Component

Projects: `Worms.Hub.Queues`, `Worms.Hub.ReplayProcessor.Queues`, `Worms.Hub.ReplayUpdater.Queues`

## IMessageQueue<T>

The core abstraction for Azure Storage Queues:

```csharp
Task<bool> HasPendingMessage();
Task EnqueueMessage(T message);
Task<(T?, MessageDetails?, ActivityContext)> DequeueMessage();
Task DeleteMessage(MessageDetails messageDetails);
```

`DequeueMessage` returns a tuple of `(payload, token, activityContext)`. The caller must call `DeleteMessage(token)` after successfully processing a message — this is the Azure Storage Queue visibility pattern (messages become visible again if not deleted within the visibility timeout).

## Message types

| Type | Queue name | Direction |
|---|---|---|
| `ReplayToProcessMessage(string ReplayFileName)` | `replays-to-process` | Gateway → WA Runner |
| `ReplayToUpdateMessage(string ReplayFileName, List<TurnGif> TurnGifs)` | `replays-to-update` | WA Runner → Gateway Worker |

## Adding a new message type

1. Add a `record` message type in `Worms.Hub.Queues`.
2. Create a concrete `MessageQueue<T>` subclass that passes the queue name (e.g. `internal sealed class ReplaysToUpdate(IConfiguration configuration) : MessageQueue<ReplayToUpdateMessage>("replays-to-update", configuration);`).
3. Register the new `IMessageQueue<NewMessage>` in `ServiceRegistration.AddQueueServices()`.

## Wire format

Messages are serialised as JSON, wrapped in a dictionary with a `"payload"` key plus W3C trace context headers (`traceparent`, `tracestate`) for OpenTelemetry propagation. The dictionary is then JSON-serialised again and base64-encoded for Azure Storage Queue compatibility.

## OpenTelemetry propagation

`MessageQueue<T>` uses `TraceContextPropagator` to:
- **Inject** the current `Activity.Context` into the message when enqueuing
- **Extract** the parent context from the message when dequeuing, then continue the trace with `ActivityKind.Consumer`

This allows distributed traces to span across queue boundaries between the gateway and the WA runner.

## Connection string

All queue clients use `IConfiguration.GetConnectionString("Storage")`. In local dev this points to Azurite; in production it points to Azure Storage.

## Local dev

Azurite emulates Azure Storage Queues locally. The queue is created automatically (`CreateIfNotExistsAsync()`) on first use — no manual setup required. Azurite runs as part of `docker compose up`.
