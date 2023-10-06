using System.Diagnostics.CodeAnalysis;
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
        var latestDetails = await _cliFiles.GetLatestDetails();
        var availablePlatforms = latestDetails.PlatformFiles.Keys.Select(x => x.ToString().ToLowerInvariant())
            .ToDictionary(x => x, x => Url.Action(action: "Get", controller: "CliFiles") + "/" + x);
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
        var latestDetails = await _cliFiles.GetLatestDetails();

        if (!Enum.TryParse<Platform>(platform, true, out var platformChecked)
            || !latestDetails.PlatformFiles.ContainsKey(platformChecked))
        {
            _logger.Log(LogLevel.Information, "Unknown CLI platform requested");
            return NotFound("Unknown platform");
        }

        var fileStream = _cliFiles.GetFileContents(platformChecked);
        var filename = latestDetails.PlatformFiles[platformChecked];
        return File(fileStream, "application/zip", filename);
    }

    [Authorize(Roles = "write:cli")]
    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<CliFileDto>> Post([FromForm] UploadCliFileDto parameters)
    {
        if (!Enum.TryParse<Platform>(parameters.Platform, true, out var platformChecked))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded");
            return BadRequest("Unknown platform");
        }

        var fileNameForDisplay = UploadUtils.GetFileNameForDisplay(parameters.File);
        _logger.Log(
            LogLevel.Information,
            "Received CLI file for {Platform} ({Filename})",
            parameters.Platform,
            fileNameForDisplay);

        if (!_cliFileValidator.IsValid(parameters.File, fileNameForDisplay))
        {
            _logger.Log(LogLevel.Warning, "Invalid CLI file uploaded");
            return BadRequest("Invalid CLI file");
        }

        await _cliFiles.SaveFileContents(parameters.File.OpenReadStream(), platformChecked);
        return await Get();
    }
}
