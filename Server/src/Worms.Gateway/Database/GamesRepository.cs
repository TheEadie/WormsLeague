using System.Collections.Generic;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Database;

public class GamesRepository : IRepository<GameDto>
{
    public IReadOnlyCollection<GameDto> Get()
    {
        return new List<GameDto>
        {
            new GameDto("1", "Pending", "Dev-Something"),
            new GameDto("2", "Complete", "Dev-2"),
        };
    }
}