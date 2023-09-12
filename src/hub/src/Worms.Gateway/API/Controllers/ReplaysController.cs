using System.Net;
using Microsoft.AspNetCore.Mvc;
using Worms.Gateway.API.DTOs;
using Worms.Gateway.API.Validators;
using Worms.Gateway.Domain;
using Worms.Gateway.Storage.Database;

namespace Worms.Gateway.API.Controllers;

internal sealed class ReplaysController : V1ApiController
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
        _logger.Log(LogLevel.Information, "Upload replay started by {Username}", username);

        var fileNameForDisplay = GetFileNameForDisplay(parameters.ReplayFile);
        _logger.Log(
            LogLevel.Information,
            "Received replay file {Name} ({Filename})",
            parameters.Name,
            fileNameForDisplay);

        if (!_replayFileValidator.IsValid(parameters.ReplayFile, fileNameForDisplay))
        {
            _logger.Log(LogLevel.Warning, "Invalid replay file uploaded by {Username}", username);
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

        var tempReplayFolderPath = _configuration["Storage:TempReplayFolder"]
            ?? throw new ArgumentException("Temp replay folder not configured");

        if (!Path.Exists(tempReplayFolderPath))
        {
            _ = Directory.CreateDirectory(tempReplayFolderPath);
        }

        var saveFilePath = Path.Combine(tempReplayFolderPath, generatedFileName);

        _logger.Log(
            LogLevel.Information,
            "Saving replay file {Filename} to {Filepath}",
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