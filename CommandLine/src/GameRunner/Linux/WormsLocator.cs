using System;
using Worms.GameRunner;

namespace worms.GameRunner.Linux
{
    internal class WormsLocator : IWormsLocator
    {
        public GameInfo Find()
        {
            return new GameInfo(false, string.Empty, string.Empty, new Version(0, 0, 0, 0));
        }
    }
}