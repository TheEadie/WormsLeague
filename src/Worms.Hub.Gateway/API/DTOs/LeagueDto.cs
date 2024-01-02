using Worms.Hub.Gateway.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

public record LeagueDto(string Id, string Name, Version Version, Uri SchemeUrl)
{
    internal static LeagueDto FromDomain(League league, Uri schemeUrl) =>
        new(league.Id, league.Name, league.Version, schemeUrl);
}
