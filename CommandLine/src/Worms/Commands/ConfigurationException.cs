using System;

namespace Worms.Commands
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException() {}

        public ConfigurationException(string message)
            : base(message) {}

        public ConfigurationException(string message, Exception inner)
            : base(message, inner) {}
    }
}
