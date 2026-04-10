using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Queues;

internal sealed class ReplaysToUpdate(IConfiguration configuration)
    : MessageQueue<ReplayToUpdateMessage>("replays-to-update", configuration);
