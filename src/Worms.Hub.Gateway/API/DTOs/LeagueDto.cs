using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record LeagueDto(string Id, string Name, Version? Version, Uri? SchemeUrl)
{
    internal static LeagueDto FromDomain(string id, string name, League? league, Uri schemeUrl) =>
        league is null
            ? new(id, name, null, null)
            : new(id, name, league.Version, schemeUrl);
}
