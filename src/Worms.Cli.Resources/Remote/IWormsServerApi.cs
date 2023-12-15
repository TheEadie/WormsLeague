namespace Worms.Cli.Resources.Remote;

internal interface IWormsServerApi
{
    Task<LatestCliDtoV1> GetLatestCliDetails();

    Task<byte[]> DownloadLatestCli(string platform);

    Task<IReadOnlyCollection<GamesDtoV1>> GetGames();

    Task<GamesDtoV1> CreateGame(CreateGameDtoV1 hostMachineName);

    Task UpdateGame(GamesDtoV1 newGameDetails);

    Task<ReplayDtoV1> CreateReplay(CreateReplayDtoV1 replayFilePath);
}
