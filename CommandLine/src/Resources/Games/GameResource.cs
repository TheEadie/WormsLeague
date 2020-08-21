using System;
using System.Collections.Generic;

namespace Worms.Resources.Games
{
    public class GameResource
    {
        public string Date { get; }
        public string Context { get; }
        public string Type { get; }
        public List<string> Teams { get; }

        public GameResource(string date, string context, string type, List<string> teams)
        {
            Date = date;
            Context = context;
            Type = type;
            Teams = teams;
        }
    }
}
