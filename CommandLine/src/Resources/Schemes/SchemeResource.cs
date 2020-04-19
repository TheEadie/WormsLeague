namespace Worms.Resources.Schemes
{
    public class SchemeResource
    {
        public string Name { get; }
        public string Context { get; }

        public SchemeResource(string name, string context)
        {
            Name = name;
            Context = context;
        }
    }
}