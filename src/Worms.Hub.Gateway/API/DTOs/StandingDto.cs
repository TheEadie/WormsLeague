using JetBrains.Annotations;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record StandingDto(string PlayerName, int Elo, int GamesPlayed);
