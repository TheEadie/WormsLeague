using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class CliController : V1ApiController
{
    private readonly ILogger<CliController> _logger;
    private readonly IConfiguration _configuration;

    public CliController(IConfiguration configuration, ILogger<CliController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("latest")]
    public async Task<ActionResult<CliInfoDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get latest CLI version started by {Username}", username);

        var cliArtifactsFolder = _configuration["Storage:CliFolder"]
            ?? throw new ArgumentException("CLI artifact folder not configured");

        var latest = await System.IO.File.ReadAllTextAsync(Path.Combine(cliArtifactsFolder, "version.txt"));

        var version = Version.TryParse(latest, out var parsedVersion)
            ? parsedVersion
            : throw new ArgumentException("Invalid version found in version.txt");

        _logger.Log(LogLevel.Information, "Get latest CLI version complete");
        return new CliInfoDto(version);
    }
}
