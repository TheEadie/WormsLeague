using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

internal sealed record LeagueDto(string Id, string Name, Version Version, Uri SchemeUrl)
{
    internal static LeagueDto FromDomain(League league, Uri schemeUrl) =>
        new(league.Id, league.Name, league.Version, schemeUrl);
}
