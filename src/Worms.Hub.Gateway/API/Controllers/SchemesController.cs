using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class SchemesController(SchemeFiles schemeFiles) : V1ApiController
{
    [HttpGet("{id}")]
    public async Task<ActionResult<SchemeDto>> Get(string id)
    {
        var latestDetails = await schemeFiles.GetLatestDetails(id).ConfigureAwait(false);
        return SchemeDto.FromDomain(
            latestDetails,
            new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative));
    }
}
