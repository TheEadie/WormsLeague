using System;

namespace GifTool.Worms
{
    public class Turn
    {
        public class Action
        {
            public Action(TimeSpan timeStamp, string description)
            {
                TimeStamp = timeStamp;
                Description = description;
            }

            public TimeSpan TimeStamp { get; }
            public string Description { get; }
        }

        public Turn(string team, Action[] weaponActionActions, TimeSpan startTime, TimeSpan endTime)
        {
            Team = team;
            WeaponActions = weaponActionActions;
            StartTime = startTime;
            EndTime = endTime;
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public string Team { get; }
        public Action[] WeaponActions { get; }
    }


}
