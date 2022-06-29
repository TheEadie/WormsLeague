using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Cli.Resources.Remote;

internal interface IWormsServerApi
{
    Task<IReadOnlyCollection<WormsServerApi.GamesDtoV1>> GetGames();
}