using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Fake;
using Worms.Armageddon.Gifs;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Folders;
using Worms.Cli.Resources.Local.Network;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Updates;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests;

internal sealed class TestHost : IDisposable
{
    public ServiceProvider Services { get; }
    private Command RootCommand { get; }
    private Runner Runner { get; }
    public IFileSystem FileSystem { get; }
    public FakeTimeProvider Time { get; }
    public IBrowserLauncher Browser { get; }
    public RecordingHttpMessageHandler Http { get; }
    public CapturingLoggerProvider Logs { get; }
    public IFolderOpener FolderOpener { get; }
    public IIpAddressLookup IpAddressLookup { get; }
    public FakeCliUpdateDownloader CliUpdateDownloader { get; }

    public IRecordingWormsArmageddon WormsArmageddon =>
        Services.GetRequiredService<IWormsArmageddon>() as IRecordingWormsArmageddon
        ?? throw new InvalidOperationException("Worms Armageddon fake is not installed in this host");

    public TestHost(bool wormsInstalled = true, bool hostCreatesReplay = true)
    {
        FileSystem = new MockFileSystem();
        Time = new FakeTimeProvider(DateTimeOffset.UtcNow);
        Browser = Substitute.For<IBrowserLauncher>();
        Http = new RecordingHttpMessageHandler();
        Logs = new CapturingLoggerProvider();
        FolderOpener = Substitute.For<IFolderOpener>();
        IpAddressLookup = Substitute.For<IIpAddressLookup>();
        IpAddressLookup.LookupForDomain(Arg.Any<string>()).Returns(new IpAddressFound("10.0.0.1"));

        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection()
            .AddHttpClient()
            .AddLogging(b => b.AddProvider(Logs).SetMinimumLevel(LogLevel.Debug))
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGifsServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices()
            .AddSingleton<IConfiguration>(configuration);

        if (wormsInstalled)
        {
            services.AddFakeInstalledWormsArmageddonServices(FileSystem, hostCreatesReplay: hostCreatesReplay);
        }
        else
        {
            services.AddFakeNotInstalledWormsArmageddonServices();
        }

        services.RemoveAll<IFileSystem>();
        services.AddSingleton(FileSystem);

        services.RemoveAll<IFolderOpener>();
        services.AddSingleton(FolderOpener);

        services.RemoveAll<IBrowserLauncher>();
        services.AddSingleton(Browser);

        var cliInfo = new CliInfo(new Version(1, 0, 0), "/cli", "worms");
        var cliInfoRetriever = Substitute.For<ICliInfoRetriever>();
        cliInfoRetriever.GetCliInfo().Returns(cliInfo);
        services.RemoveAll<ICliInfoRetriever>();
        services.AddSingleton(cliInfoRetriever);

        // Seed a stub binary at the fake CLI's reported install path so
        // CliUpdater.InstallUpdate can move it to a .bak file.
        FileSystem.Directory.CreateDirectory(cliInfo.Folder);
        FileSystem.File.WriteAllBytes(FileSystem.Path.Combine(cliInfo.Folder, cliInfo.FileName), []);

        CliUpdateDownloader = new FakeCliUpdateDownloader(FileSystem, cliInfo.FileName);
        services.RemoveAll<ICliUpdateDownloader>();
        services.AddSingleton<ICliUpdateDownloader>(CliUpdateDownloader);

        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(Time);

        services.RemoveAll<IIpAddressLookup>();
        services.AddSingleton(IpAddressLookup);

        services.AddHttpClient(Options.DefaultName)
            .ConfigurePrimaryHttpMessageHandler(_ => Http);

        Services = services.BuildServiceProvider();
        RootCommand = CliStructure.BuildCommandLine(Services);
        Runner = Services.GetRequiredService<Runner>();
    }

    public Task<int> Run(params string[] args) =>
        Runner.Run(RootCommand, args, CancellationToken.None);

    public void Dispose() => Services.Dispose();
}
