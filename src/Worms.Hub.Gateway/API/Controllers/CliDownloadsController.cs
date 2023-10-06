using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/downloads/cli")]
internal sealed class CliDownloadsController : V1ApiController
{
    private readonly ILogger<CliDownloadsController> _logger;
    private readonly CliArtifacts _cliArtifacts;
    private readonly CliFileValidator _cliFileValidator;

    public CliDownloadsController(
        CliArtifacts cliArtifacts,
        CliFileValidator cliFileValidator,
        ILogger<CliDownloadsController> logger)
    {
        _cliArtifacts = cliArtifacts;
        _cliFileValidator = cliFileValidator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CliInfoDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get latest CLI version started by {Username}", username);

        var latestDetails = await _cliArtifacts.GetLatestDetails();
        var availablePlatforms = latestDetails.PlatformFiles.Keys.Select(x => x.ToString().ToLowerInvariant())
            .ToDictionary(x => x, x => Url.Action(action: "Get", controller: "CliDownloads") + "/" + x);

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return new CliInfoDto(latestDetails.Version, availablePlatforms);
    }

    [AllowAnonymous]
    [HttpGet("{platform}")]
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Stream is disposed by File object")]
    public async Task<ActionResult<CliInfoDto>> Get(string platform)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Download CLI for {Platform} started by {Username}", platform, username);

        var latestDetails = await _cliArtifacts.GetLatestDetails();

        if (!Enum.TryParse<Platform>(platform, true, out var platformChecked)
            || !latestDetails.PlatformFiles.ContainsKey(platformChecked))
        {
            return NotFound("Unknown platform");
        }

        var fileStream = _cliArtifacts.GetFileContents(platformChecked);
        var filename = latestDetails.PlatformFiles[platformChecked];

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return File(fileStream, "application/zip", filename);
    }

    [Authorize(Roles = "write:cli")]
    [HttpPost]
    public async Task<ActionResult<CliInfoDto>> Post([FromForm] UploadCliDto parameters)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Upload CLI started by {Username}", username);

        if (!Enum.TryParse<Platform>(parameters.Platform, true, out var platformChecked))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded by {Username}", username);
            return BadRequest("Unknown platform");
        }

        var fileNameForDisplay = GetFileNameForDisplay(parameters.CliFile);
        _logger.Log(
            LogLevel.Information,
            "Received CLI file for {Platform} ({Filename})",
            parameters.Platform,
            fileNameForDisplay);

        if (!_cliFileValidator.IsValid(parameters.CliFile, fileNameForDisplay))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded by {Username}", username);
            return BadRequest("Invalid CLI file");
        }

        await _cliArtifacts.SaveFileContents(parameters.CliFile.OpenReadStream(), platformChecked);

        _logger.Log(LogLevel.Information, "Upload CLI complete");
        return await Get();
    }

    private static string GetFileNameForDisplay(IFormFile file)
    {
        var untrustedFileName = Path.GetFileName(file.FileName);
        return WebUtility.HtmlEncode(untrustedFileName);
    }
}
