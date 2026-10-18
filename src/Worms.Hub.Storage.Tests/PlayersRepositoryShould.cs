using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Tests;

[TestFixture]
[Category("Integration")]
internal sealed class PlayersRepositoryShould
{
    private ServiceProvider _services = null!;
    private IPlayersRepository _repository = null!;

    [OneTimeSetUp]
    public void CreateRepository()
    {
        _services = ComposeDatabase.CreateHubStorageServices();
        _repository = _services.GetRequiredService<IPlayersRepository>();
    }

    [OneTimeTearDown]
    public void DisposeServices() => _services.Dispose();

    [Test]
    public void ReturnTheCreatedPlayerAndFindItByAuthSubject()
    {
        var authSubject = UniqueAuthSubject();

        var created = _repository.Create(new Player(authSubject, "Contract Test Player"));

        created.ShouldBe(new Player(authSubject, "Contract Test Player"));
        _repository.GetByAuthSubject(authSubject).ShouldBe(new Player(authSubject, "Contract Test Player"));
    }

    [Test]
    public void ReturnNoPlayerForANonExistentAuthSubject()
    {
        var authSubject = UniqueAuthSubject();

        var player = Should.NotThrow(() => _repository.GetByAuthSubject(authSubject));

        player.ShouldBeNull();
    }

    [Test]
    public void RejectADuplicateAuthSubjectAndKeepTheOriginalPlayer()
    {
        var authSubject = UniqueAuthSubject();
        _ = _repository.Create(new Player(authSubject, "Original Name"));

        var exception = Should.Throw<PostgresException>(
            () => _repository.Create(new Player(authSubject, "Duplicate Name")));

        exception.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);

        _repository.GetByAuthSubject(authSubject).ShouldBe(new Player(authSubject, "Original Name"));
    }

    private static string UniqueAuthSubject() => $"contract-test|{Guid.NewGuid():N}";
}
