using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Queues;
using Worms.Hub.ReplayProcessor.Queue;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class ReplaysController(
    IRepository<Replay> repository,
    IMessageQueue<ReplayToProcessMessage> replayProcessor,
    ReplayFileValidator replayFileValidator,
    ReplayFiles replayFiles,
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

        var tempFilename = await replayFiles.SaveFileContents(parameters.ReplayFile.OpenReadStream());
        var replay = repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename, null));

        // Enqueue the replay for processing
        var message = new ReplayToProcessMessage(replay.Id);
        await replayProcessor.EnqueueMessage(message);

        return ReplayDto.FromDomain(replay);
    }
}
