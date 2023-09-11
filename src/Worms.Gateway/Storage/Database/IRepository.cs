namespace Worms.Gateway.Storage.Database;

public interface IRepository<T>
{
    IReadOnlyCollection<T> GetAll();

    T Create(T item);

    void Update(T item);
}
