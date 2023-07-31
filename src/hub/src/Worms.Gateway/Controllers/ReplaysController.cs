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
using Worms.Gateway.Validators;

namespace Worms.Gateway.Controllers;

public class ReplaysController : V1ApiController
{
    private readonly IRepository<Replay> _repository;
    private readonly ReplayFileValidator _replayFileValidator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReplaysController> _logger;

    public ReplaysController(
        IRepository<Replay> repository,
        ReplayFileValidator replayFileValidator,
        IConfiguration configuration,
        ILogger<ReplaysController> logger)
    {
        _repository = repository;
        _replayFileValidator = replayFileValidator;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ReplayDto>> Post([FromForm] CreateReplayDto parameters)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Upload replay started by {username}", username);

        var fileNameForDisplay = GetFileNameForDisplay(parameters.ReplayFile);
        _logger.Log(
            LogLevel.Information,
            "Received replay file {name} ({filename})",
            parameters.Name,
            fileNameForDisplay);

        if (!_replayFileValidator.IsValid(parameters.ReplayFile, fileNameForDisplay))
        {
            _logger.Log(LogLevel.Warning, "Invalid replay file uploaded by {username}", username);
            return BadRequest("Invalid replay file");
        }

        var tempFilename = await SaveFileToTempLocation(parameters.ReplayFile, fileNameForDisplay);
        var replay = _repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename));

        _logger.Log(LogLevel.Information, "Upload of replay complete");
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
