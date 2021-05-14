using System;
using System.Collections.Generic;

namespace Worms.Armageddon.Resources.Replays
{
    public record ReplayResource(
        DateTime Date,
        string Context,
        bool Processed,
        List<string> Teams,
        string Winner,
        string FullLog);
}
