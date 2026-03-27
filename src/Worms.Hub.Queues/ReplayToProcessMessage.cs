using JetBrains.Annotations;

namespace Worms.Hub.Queues;

[PublicAPI]
public record ReplayToProcessMessage(string ReplayFileName);
