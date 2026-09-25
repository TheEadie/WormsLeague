# Hub Queues Component

Projects: `Worms.Hub.Queues`, `Worms.Hub.Queues.Fake`

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
| `ReplayToUpdateMessage(string ReplayFileName, IReadOnlyList<TurnGif>? TurnGifs = null)` | `replays-to-update` | WA Runner → Gateway Worker |

`TurnGif(int TurnNumber, string GifFileName)` is defined alongside the message and identifies the GIF the runner produced for a given turn.

## Adding a new message type

1. Add a `record` message type in `Worms.Hub.Queues`.
2. Create a concrete `MessageQueue<T>` subclass that passes the queue name (e.g. `internal sealed class ReplaysToUpdate(IConfiguration configuration) : MessageQueue<ReplayToUpdateMessage>("replays-to-update", configuration);`).
3. Register the new `IMessageQueue<NewMessage>` in `ServiceRegistration.AddQueueServices()`.
4. Register a `FakeMessageQueue<NewMessage>` in `Worms.Hub.Queues.Fake`'s `AddFakeQueueServices()`.

## Worms.Hub.Queues.Fake

`FakeMessageQueue<T>` is an in-memory `IMessageQueue<T>` for unit tests. `EnqueueMessage` adds to `Pending`. `DequeueMessage` returns the oldest pending message (or `(null, null, default)` when empty) and leaves it pending, like a real queue before the visibility timeout. `DeleteMessage` moves it to `Deleted`, so it is not returned again, and throws if the token doesn't match a pending message. Tests can construct it directly or call `AddFakeQueueServices()` (as the worker `ProcessorShould` tests do, on top of the production `AddWorkerServices()`), which replaces both `IMessageQueue<ReplayToProcessMessage>` and `IMessageQueue<ReplayToUpdateMessage>` via `RemoveAll<>` + `AddSingleton` and also registers each concrete `FakeMessageQueue<T>` so tests can resolve it.

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
