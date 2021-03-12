using System;
using System.Collections.Generic;

namespace Worms.Armageddon.Resources.Replays
{
    public class ReplayResource
    {
        public DateTime Date { get; }
        public string Context { get; }
        public bool Processed { get; }
        public List<string> Teams { get; }
        public string Winner { get; }
        public string FullLog { get; }

        public ReplayResource(DateTime date, string context, bool processed, List<string> teams, string winner, string fullLog)
        {
            Date = date;
            Context = context;
            Processed = processed;
            Teams = teams;
            Winner = winner;
            FullLog = fullLog;
        }
    }
}
