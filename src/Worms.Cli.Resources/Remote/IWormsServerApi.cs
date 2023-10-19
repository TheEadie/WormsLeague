namespace Worms.Cli.Resources.Remote;

internal interface IWormsServerApi
{
    Task<WormsServerApi.LatestCliDtoV1> GetLatestCliDetails();

    Task<IReadOnlyCollection<WormsServerApi.GamesDtoV1>> GetGames();

    Task<WormsServerApi.GamesDtoV1> CreateGame(WormsServerApi.CreateGameDtoV1 hostMachineName);

    Task UpdateGame(WormsServerApi.GamesDtoV1 newGameDetails);

    Task<WormsServerApi.ReplayDtoV1> CreateReplay(WormsServerApi.CreateReplayDtoV1 replayFilePath);
}
