namespace Worms.Cli.Configuration;

public interface IConfigManager
{
    Config Load();

    void Save(Config config);
}
