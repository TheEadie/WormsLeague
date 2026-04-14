using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/files/game")]
internal sealed class GameFilesController(
    GameFiles gameFiles,
    ILogger<GameFilesController> logger) : V1ApiController
{
    [Authorize(Roles = "download:game")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var gameFolder = gameFiles.GameFolderPath;

        if (!Directory.Exists(gameFolder))
        {
            logger.Log(LogLevel.Warning, "Game folder not found at {Path}", gameFolder);
            return NotFound();
        }

        var memoryStream = new MemoryStream();
        await using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in Directory.GetFiles(gameFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(gameFolder, file);
                var entry = archive.CreateEntry(relativePath);
                await using var entryStream = await entry.OpenAsync();
                await using var fileStream = System.IO.File.OpenRead(file);
                await fileStream.CopyToAsync(entryStream);
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream, "application/zip", "wa-game.zip");
    }
}
