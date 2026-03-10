using JetBrains.Annotations;

namespace Worms.Hub.Queues;

[PublicAPI]
public record MessageDetails(string MessageId, string PopReceipt);
