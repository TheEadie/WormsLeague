using System.Collections.Generic;

namespace Worms.Gateway.Database;

public interface IRepository<out T>
{
    IReadOnlyCollection<T> Get();
}