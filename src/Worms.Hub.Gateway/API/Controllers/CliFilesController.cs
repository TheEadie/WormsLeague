using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/files/cli")]
internal sealed class CliFilesController : V1ApiController
{
    private readonly ILogger<CliFilesController> _logger;
    private readonly CliFiles _cliFiles;
    private readonly CliFileValidator _cliFileValidator;

    public CliFilesController(CliFiles cliFiles, CliFileValidator cliFileValidator, ILogger<CliFilesController> logger)
    {
        _cliFiles = cliFiles;
        _cliFileValidator = cliFileValidator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CliFileDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get latest CLI version started by {Username}", username);

        var latestDetails = await _cliFiles.GetLatestDetails();
        var availablePlatforms = latestDetails.PlatformFiles.Keys.Select(x => x.ToString().ToLowerInvariant())
            .ToDictionary(x => x, x => Url.Action(action: "Get", controller: "CliFiles") + "/" + x);

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return new CliFileDto(latestDetails.Version, availablePlatforms);
    }

    [AllowAnonymous]
    [HttpGet("{platform}")]
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Stream is disposed by File object")]
    public async Task<ActionResult<CliFileDto>> Get(string platform)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Download CLI for {Platform} started by {Username}", platform, username);

        var latestDetails = await _cliFiles.GetLatestDetails();

        if (!Enum.TryParse<Platform>(platform, true, out var platformChecked)
            || !latestDetails.PlatformFiles.ContainsKey(platformChecked))
        {
            return NotFound("Unknown platform");
        }

        var fileStream = _cliFiles.GetFileContents(platformChecked);
        var filename = latestDetails.PlatformFiles[platformChecked];

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return File(fileStream, "application/zip", filename);
    }

    [Authorize(Roles = "write:cli")]
    [HttpPost]
    public async Task<ActionResult<CliFileDto>> Post([FromForm] UploadCliFileDto parameters)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Upload CLI started by {Username}", username);

        if (!Enum.TryParse<Platform>(parameters.Platform, true, out var platformChecked))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded by {Username}", username);
            return BadRequest("Unknown platform");
        }

        var fileNameForDisplay = GetFileNameForDisplay(parameters.File);
        _logger.Log(
            LogLevel.Information,
            "Received CLI file for {Platform} ({Filename})",
            parameters.Platform,
            fileNameForDisplay);

        if (!_cliFileValidator.IsValid(parameters.File, fileNameForDisplay))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded by {Username}", username);
            return BadRequest("Invalid CLI file");
        }

        await _cliFiles.SaveFileContents(parameters.File.OpenReadStream(), platformChecked);

        _logger.Log(LogLevel.Information, "Upload CLI complete");
        return await Get();
    }

    private static string GetFileNameForDisplay(IFormFile file)
    {
        var untrustedFileName = Path.GetFileName(file.FileName);
        return WebUtility.HtmlEncode(untrustedFileName);
    }
}
