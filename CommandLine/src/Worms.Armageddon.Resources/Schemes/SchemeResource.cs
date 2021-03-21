using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes
{
    public class SchemeResource
    {
        public string Name { get; }
        public string Context { get; }
        public Scheme Details { get; }

        public SchemeResource(string name, string context, Scheme details)
        {
            Name = name;
            Context = context;
            Details = details;
        }
    }
}
