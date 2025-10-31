using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Queues;

internal sealed class ReplaysToProcess(IConfiguration configuration)
    : MessageQueue<ReplayToProcessMessage>("replays-to-process", configuration);
