using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Tests;

[TestFixture]
[Category("Integration")]
internal sealed class GamesRepositoryShould
{
    private const string NonExistentId = "-1";

    private ServiceProvider _services = null!;
    private IRepository<Game> _repository = null!;

    [OneTimeSetUp]
    public void CreateRepository()
    {
        _services = new ServiceCollection()
            .AddSingleton<IConfiguration>(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:Database"] = ComposeDatabase.ConnectionString
                        })
                    .Build())
            .AddHubStorageServices()
            .BuildServiceProvider();
        _repository = _services.GetRequiredService<IRepository<Game>>();
    }

    [OneTimeTearDown]
    public void DisposeServices() => _services.Dispose();

    [Test]
    public void ReturnADatabaseAssignedIdWhenCreatingAGame()
    {
        var hostMachine = UniqueHostMachine();

        var created = _repository.Create(new Game("0", "Pending", hostMachine));

        created.Id.ShouldNotBe("0");
        int.TryParse(created.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out _).ShouldBeTrue();
        _repository.GetAll().ShouldContain(new Game(created.Id, "Pending", hostMachine));
    }

    [Test]
    public void IgnoreTheSuppliedIdWhenCreatingAGame()
    {
        const string suppliedId = "999999999";

        var first = _repository.Create(new Game(suppliedId, "Pending", UniqueHostMachine()));
        var second = _repository.Create(new Game(suppliedId, "Pending", UniqueHostMachine()));

        first.Id.ShouldNotBe(suppliedId);
        second.Id.ShouldNotBe(suppliedId);
        second.Id.ShouldNotBe(first.Id);
    }

    [Test]
    public void IncludeCreatedGamesInGetAll()
    {
        var firstHost = UniqueHostMachine();
        var secondHost = UniqueHostMachine();
        var first = _repository.Create(new Game("0", "Pending", firstHost));
        var second = _repository.Create(new Game("0", "Complete", secondHost));

        var all = _repository.GetAll();

        all.ShouldContain(new Game(first.Id, "Pending", firstHost));
        all.ShouldContain(new Game(second.Id, "Complete", secondHost));
    }

    [Test]
    public void PersistUpdatedStatusAndHostMachine()
    {
        var created = _repository.Create(new Game("0", "Pending", UniqueHostMachine()));
        var updatedHost = UniqueHostMachine();

        _repository.Update(created with { Status = "Complete", HostMachine = updatedHost });

        _repository.GetAll().Single(x => x.Id == created.Id).ShouldBe(new Game(created.Id, "Complete", updatedHost));
    }

    [Test]
    public void LeaveOtherGamesUnchangedWhenUpdatingANonExistentId()
    {
        var first = _repository.Create(new Game("0", "Pending", UniqueHostMachine()));
        var second = _repository.Create(new Game("0", "Complete", UniqueHostMachine()));

        Should.NotThrow(() => _repository.Update(new Game(NonExistentId, "Updated", UniqueHostMachine())));

        var all = _repository.GetAll();
        all.ShouldContain(first);
        all.ShouldContain(second);
        all.ShouldNotContain(x => x.Id == NonExistentId);
    }

    private static string UniqueHostMachine() => $"contract-test-{Guid.NewGuid():N}";
}
