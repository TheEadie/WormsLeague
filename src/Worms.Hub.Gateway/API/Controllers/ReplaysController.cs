using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Database;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class ReplaysController(
    IRepository<Replay> repository,
    ReplayFileValidator replayFileValidator,
    IConfiguration configuration,
    ILogger<ReplaysController> logger) : V1ApiController
{
    [HttpPost]
    public async Task<ActionResult<ReplayDto>> Post([FromForm] CreateReplayDto parameters)
    {
        var fileNameForDisplay = UploadUtils.GetFileNameForDisplay(parameters.ReplayFile);
        logger.Log(
            LogLevel.Information,
            "Received replay file {Name} ({Filename})",
            parameters.Name,
            fileNameForDisplay);

        if (!replayFileValidator.IsValid(parameters.ReplayFile, fileNameForDisplay))
        {
            logger.Log(LogLevel.Warning, "Invalid replay file uploaded");
            return BadRequest("Invalid replay file");
        }

        var tempFilename = await SaveFileToTempLocation(parameters.ReplayFile, fileNameForDisplay);
        var replay = repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename));
        return ReplayDto.FromDomain(replay);
    }

    private async Task<string> SaveFileToTempLocation(IFormFile replayFile, string fileNameForDisplay)
    {
        var generatedFileName = Path.GetRandomFileName();

        var tempReplayFolderPath = configuration["Storage:TempReplayFolder"]
            ?? throw new ArgumentException("Temp replay folder not configured");

        if (!Path.Exists(tempReplayFolderPath))
        {
            _ = Directory.CreateDirectory(tempReplayFolderPath);
        }

        var saveFilePath = Path.Combine(tempReplayFolderPath, generatedFileName);

        logger.Log(
            LogLevel.Information,
            "Saving replay file {Filename} to {Filepath}",
            fileNameForDisplay,
            saveFilePath);

        await using var stream = System.IO.File.Create(saveFilePath);
        await replayFile.CopyToAsync(stream);
        return generatedFileName;
    }
}
