using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Armageddon.Gifs;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests;

internal sealed class TestHost : IDisposable
{
    public ServiceProvider Services { get; }
    private Command RootCommand { get; }
    private Runner Runner { get; }
    private MockFileSystem FileSystem { get; }
    public FakeTimeProvider Time { get; }
    public RecordingBrowserLauncher Browser { get; }
    public RecordingHttpMessageHandler Http { get; }
    public CapturingLoggerProvider Logs { get; }

    public TestHost()
    {
        FileSystem = new MockFileSystem();
        Time = new FakeTimeProvider(DateTimeOffset.UtcNow);
        Browser = new RecordingBrowserLauncher();
        Http = new RecordingHttpMessageHandler();
        Logs = new CapturingLoggerProvider();

        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection()
            .AddHttpClient()
            .AddLogging(b => b.AddProvider(Logs).SetMinimumLevel(LogLevel.Debug))
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
            .AddWormsArmageddonGifsServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices()
            .AddSingleton<IConfiguration>(configuration);

        services.RemoveAll<IFileSystem>();
        services.AddSingleton<IFileSystem>(FileSystem);

        services.RemoveAll<IBrowserLauncher>();
        services.AddSingleton<IBrowserLauncher>(Browser);

        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(Time);

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
