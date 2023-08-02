namespace Worms.Gateway.Database;

public interface IRepository<T>
{
    IReadOnlyCollection<T> GetAll();

    T Create(T item);

    void Update(T item);
}
