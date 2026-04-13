using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Storage.Files;

public sealed class GameFiles(IConfiguration configuration)
{
    public string GetGameFolderPath() =>
        configuration["Storage:GameFolder"]
            ?? throw new ArgumentException("Game folder not configured");
}
