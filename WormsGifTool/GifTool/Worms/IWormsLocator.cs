namespace GifTool.Worms
{
    public interface IWormsLocator
    {
        string VideoLocation { get; }
        string GamesLocation { get; }
        string ExeLocation { get; }
        string ProcessName { get; }
    }
}
