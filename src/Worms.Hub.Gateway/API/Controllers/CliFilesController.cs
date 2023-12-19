using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/files/cli")]
internal sealed class CliFilesController(
    CliFiles cliFiles,
    CliFileValidator cliFileValidator,
    ILogger<CliFilesController> logger) : V1ApiController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CliFileDto>> Get()
    {
        var latestDetails = await cliFiles.GetLatestDetails().ConfigureAwait(false);
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
        var latestDetails = await cliFiles.GetLatestDetails().ConfigureAwait(false);

        if (!Enum.TryParse<Platform>(platform, true, out var platformChecked)
            || !latestDetails.PlatformFiles.TryGetValue(platformChecked, out _))
        {
            logger.Log(LogLevel.Information, "Unknown CLI platform requested");
            return NotFound("Unknown platform");
        }

        var fileStream = cliFiles.GetFileContents(platformChecked);
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
            logger.Log(LogLevel.Warning, "Invalid CLI file uploaded - unknown platform");
            return BadRequest("Unknown platform");
        }

        var fileNameForDisplay = UploadUtils.GetFileNameForDisplay(parameters.File);
        logger.Log(
            LogLevel.Information,
            "Received CLI file for {Platform}, version: {Version} ({Filename})",
            parameters.Platform,
            parameters.Version,
            fileNameForDisplay);

        if (!cliFileValidator.IsValid(parameters.File, fileNameForDisplay))
        {
            logger.Log(LogLevel.Warning, "Invalid CLI file uploaded - invalid file");
            return BadRequest("Invalid CLI file");
        }

        await cliFiles.SaveLatestVersion(parameters.Version).ConfigureAwait(false);
        await cliFiles.SaveFileContents(parameters.File.OpenReadStream(), platformChecked).ConfigureAwait(false);
        return await Get().ConfigureAwait(false);
    }
}
