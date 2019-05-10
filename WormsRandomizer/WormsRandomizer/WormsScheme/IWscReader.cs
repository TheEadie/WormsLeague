namespace WormsRandomizer.WormsScheme
{
    public interface IWscReader
    {
        IReadOnlyScheme Read(string filePath);
    }
}