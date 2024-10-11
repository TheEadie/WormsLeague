using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class LeaguesController(SchemeFiles schemeFiles) : V1ApiController
{
    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueDto>> Get(string id)
    {
        var latestDetails = await schemeFiles.GetLatestDetails(id).ConfigureAwait(false);
        return LeagueDto.FromDomain(
            latestDetails,
            new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative));
    }
}
