namespace Worms.Configuration
{
    public interface IConfigManager
    {
        Config Load();
        void Save(Config config);
    }
}
