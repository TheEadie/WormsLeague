using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/downloads/cli")]
internal sealed class CliDownloadsController : V1ApiController
{
    private readonly ILogger<CliDownloadsController> _logger;
    private readonly CliArtifacts _cliArtifacts;

    public CliDownloadsController(CliArtifacts cliArtifacts, ILogger<CliDownloadsController> logger)
    {
        _cliArtifacts = cliArtifacts;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CliInfoDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get latest CLI version started by {Username}", username);

        var latestDetails = await _cliArtifacts.GetLatest();
        var availablePlatforms = latestDetails.PlatformFiles.Keys.Select(x => x.ToString().ToLowerInvariant())
            .ToDictionary(x => x, x => Url.Action(action: "Get", controller: "CliDownloads") + "/" + x);

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return new CliInfoDto(latestDetails.Version, availablePlatforms);
    }

    [AllowAnonymous]
    [HttpGet("{platform}")]
    [SuppressMessage(
        "Security",
        "CA3003:Review code for file path injection vulnerabilities",
        Justification = "Platform is validated against the enum before being used in the path")]
    public async Task<ActionResult<CliInfoDto>> Get(string platform)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Download CLI for {Platform} started by {Username}", platform, username);

        var latestDetails = await _cliArtifacts.GetLatest();

        if (!Enum.TryParse<Platform>(platform, true, out var platformChecked)
            || !latestDetails.PlatformFiles.ContainsKey(platformChecked))
        {
            return NotFound();
        }

        var filePath = latestDetails.PlatformFiles[platformChecked];
        var fileContents = await System.IO.File.ReadAllBytesAsync(filePath);
        var filename = Path.GetFileName(filePath);
        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return File(fileContents, "application/zip", filename);
    }
}
