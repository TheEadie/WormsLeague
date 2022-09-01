using System.Collections.Generic;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Database;

public interface IRepository<T>
{
    IReadOnlyCollection<T> Get();
    T Create(T item);
    void Update(T item);
}