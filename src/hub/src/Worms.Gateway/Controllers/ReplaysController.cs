using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Worms.Gateway.Database;
using Worms.Gateway.Domain;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers;

public class ReplaysController : V1ApiController
{
    private readonly IRepository<Replay> _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReplaysController> _logger;

    public ReplaysController(
        IRepository<Replay> repository,
        IConfiguration configuration,
        ILogger<ReplaysController> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ReplayDto>> Post([FromForm] CreateReplayDto parameters)
    {
        var fileNameForDisplay = GetFileNameForDisplay(parameters.ReplayFile);
        _logger.Log(
            LogLevel.Information,
            "Received replay file {filename} with name {name}",
            fileNameForDisplay,
            parameters.Name);
        var tempFilename = await SaveFileToTempLocation(parameters.ReplayFile, fileNameForDisplay);
        var replay = _repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename));
        return ReplayDto.FromDomain(replay);
    }

    private async Task<string> SaveFileToTempLocation(IFormFile replayFile, string fileNameForDisplay)
    {
        var generatedFileName = Path.GetRandomFileName();

        var tempReplayFolderPath = _configuration["Storage:TempReplayFolder"];
        if (tempReplayFolderPath is null)
        {
            throw new Exception("Temp replay folder not configured");
        }

        if (!Path.Exists(tempReplayFolderPath))
        {
            Directory.CreateDirectory(tempReplayFolderPath);
        }

        var saveFilePath = Path.Combine(tempReplayFolderPath, generatedFileName);

        _logger.Log(
            LogLevel.Information,
            "Saving replay file {filename} to {filepath}",
            fileNameForDisplay,
            saveFilePath);

        await using var stream = System.IO.File.Create(saveFilePath);
        await replayFile.CopyToAsync(stream);
        return generatedFileName;
    }

    private static string GetFileNameForDisplay(IFormFile replayFile)
    {
        var untrustedFileName = Path.GetFileName(replayFile.FileName);
        return WebUtility.HtmlEncode(untrustedFileName);
    }
}
