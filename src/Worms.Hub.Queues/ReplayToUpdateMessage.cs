using JetBrains.Annotations;

namespace Worms.Hub.Queues;

[PublicAPI]
public record ReplayToUpdateMessage(string ReplayFileName);
