using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Storage.Files;

public sealed class GameFiles(IConfiguration configuration)
{
    public string GameFolderPath { get; } =
        configuration["Storage:GameFolder"]
            ?? throw new ArgumentException("Game folder not configured");
}
