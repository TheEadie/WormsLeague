using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers;

public class ReplaysController : V1ApiController
{
    [HttpPost]
    public ActionResult<ReplayDto> Post(IFormFile replayFile)
    {
        Console.WriteLine(replayFile.FileName);
        return new ReplayDto("0");
    }
}