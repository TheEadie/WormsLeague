using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class CliController : V1ApiController
{
    private readonly ILogger<CliController> _logger;
    private readonly CliArtifacts _cliArtifacts;

    public CliController(CliArtifacts cliArtifacts, ILogger<CliController> logger)
    {
        _cliArtifacts = cliArtifacts;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("latest")]
    public async Task<ActionResult<CliInfoDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get latest CLI version started by {Username}", username);

        var latestDetails = await _cliArtifacts.GetLatest();

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return new CliInfoDto(latestDetails.Version);
    }

    [AllowAnonymous]
    [HttpGet("download/{platform}")]
    public async Task<ActionResult<CliInfoDto>> Get(string platform)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Download CLI for {Platform} started by {Username}", platform, username);

        var latestDetails = await _cliArtifacts.GetLatest();

        if (!Enum.TryParse<Platform>(platform, out var platformChecked)
            || !latestDetails.PlatformFiles.ContainsKey(platformChecked))
        {
            return NotFound();
        }

        var filePath = latestDetails.PlatformFiles[platformChecked];
#pragma warning disable CA3003 // values for filePath can only be from the pre-defined list in CliArtifacts
        var fileContents = await System.IO.File.ReadAllBytesAsync(filePath);
#pragma warning restore CA3003
        var filename = Path.GetFileName(filePath);
        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return File(fileContents, "application/zip", filename);
    }
}
