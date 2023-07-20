using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers;

public class ReplaysController : V1ApiController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReplaysController> _logger;

    public ReplaysController(IConfiguration configuration, ILogger<ReplaysController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<ReplayDto>> Post(IFormFile replayFile)
    {
        var fileNameForDisplay = GetFileNameForDisplay(replayFile);
        await SaveFileToTempLocation(replayFile, fileNameForDisplay);

        // TODO Store replay record in database
        
        // TODO Fire event to process replay
        
        return new ReplayDto("0");
    }

    private async Task SaveFileToTempLocation(IFormFile replayFile, string fileNameForDisplay)
    {
        var generatedFileName = Path.GetRandomFileName();

        var tempReplayFolderPath = _configuration["Storage:TempReplayFolder"];
        if (tempReplayFolderPath is null)
        {
            throw new Exception("Temp replay folder not configured");
        }

        var saveFilePath = Path.Combine(tempReplayFolderPath, generatedFileName);

        _logger.Log(LogLevel.Information, "Saving replay file {fileNameForDisplay} to {saveFilePath}",
            fileNameForDisplay, saveFilePath);

        await using var stream = System.IO.File.Create(saveFilePath);
        await replayFile.CopyToAsync(stream);
    }

    private static string GetFileNameForDisplay(IFormFile replayFile)
    {
        var untrustedFileName = Path.GetFileName(replayFile.FileName);
        return WebUtility.HtmlEncode(untrustedFileName);
    }
}