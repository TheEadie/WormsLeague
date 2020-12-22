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

        public GameResource(DateTime date, string context, bool processed, List<string> teams)
        {
            Date = date;
            Context = context;
            Processed = processed;
            Teams = teams;
        }
    }
}
