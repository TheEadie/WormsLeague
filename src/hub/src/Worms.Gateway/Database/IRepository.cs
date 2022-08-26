using System.Collections.Generic;

namespace Worms.Gateway.Database;

public interface IRepository<T>
{
    IReadOnlyCollection<T> Get();
    T Create(T game);
}