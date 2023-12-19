namespace Worms.Cli.Resources.Remote;

internal interface IWormsServerApi
{
    Task<LatestCliDtoV1> GetLatestCliDetails();

    Task<byte[]> DownloadLatestCli(string platform);

    Task<SchemeDtoV1> GetScheme(string id);

    Task<byte[]> DownloadScheme(string id);

    Task<IReadOnlyCollection<GamesDtoV1>> GetGames();

    Task<GamesDtoV1> CreateGame(CreateGameDtoV1 createParams);

    Task UpdateGame(GamesDtoV1 newGameDetails);

    Task<ReplayDtoV1> CreateReplay(CreateReplayDtoV1 createParams);
}
