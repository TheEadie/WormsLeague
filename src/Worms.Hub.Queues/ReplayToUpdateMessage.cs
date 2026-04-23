using JetBrains.Annotations;

namespace Worms.Hub.Queues;

[PublicAPI]
public record ReplayToUpdateMessage(string ReplayFileName, IReadOnlyList<TurnGif>? TurnGifs = null);

[PublicAPI]
public record TurnGif(int TurnNumber, string GifFileName);
