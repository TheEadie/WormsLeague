namespace WormsRandomizer.WormsScheme
{
    public interface IWscWriter
    {
        void Write(IReadOnlyScheme scheme, string filePath);
    }
}