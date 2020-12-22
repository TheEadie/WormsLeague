using System;
using System.Collections.Generic;

namespace Worms.Resources.Games
{
    public class GameResource
    {
        public DateTime Date { get; }
        public string Context { get; }
        public bool Processed { get; }
        public List<string> Teams { get; }
        public string Winner { get; }
        public string FullLog { get; }

        public GameResource(DateTime date, string context, bool processed, List<string> teams, string winner, string fullLog)
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
