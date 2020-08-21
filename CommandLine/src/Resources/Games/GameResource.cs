namespace Worms.Resources.Games
{
    public class GameResource
    {
        public string Name { get; }
        public string Context { get; }

        public GameResource(string name, string context)
        {
            Name = name;
            Context = context;
        }
    }
}
