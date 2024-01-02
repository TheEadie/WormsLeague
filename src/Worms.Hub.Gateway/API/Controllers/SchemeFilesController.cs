using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

[Route("~/api/v{version:apiVersion}/files/schemes")]
internal sealed class SchemeFilesController(SchemeFiles schemeFiles, ILogger<SchemeFilesController> logger)
    : V1ApiController
{
    [HttpGet("{id}")]
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Stream is disposed by File object")]
    [SuppressMessage(
        "Security",
        "CA3003:Review code for file path injection vulnerabilities",
        Justification = "User input is checked against known values list")]
    public async Task<ActionResult<LeagueDto>> Get(string id)
    {
        // Temp guard to only allow well known Schemes for now
        // Will need to look up Scheme in DB and check if user has access to it in future
        if (!KnownSchemeNames.Contains(id, StringComparer.InvariantCultureIgnoreCase))
        {
            logger.Log(LogLevel.Information, "Unknown scheme file {Name} requested", id);
            return NotFound("Unknown scheme file");
        }

        var latestDetails = await schemeFiles.GetLatestDetails(id).ConfigureAwait(false);
        if (!System.IO.File.Exists(latestDetails.SchemePath))
        {
            logger.Log(LogLevel.Information, "Scheme file {Name} not found", id);
            return NotFound("Scheme file not found");
        }

        var fileStream = schemeFiles.GetFileContents(id);
        return File(fileStream, "application/zip", $"{id}.{latestDetails.Version}.wsc");
    }

    private static IEnumerable<string> KnownSchemeNames => new[] { "redgate" };
}
