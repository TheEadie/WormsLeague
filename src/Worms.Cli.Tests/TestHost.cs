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
using Worms.Armageddon.Game.Fake;
using Worms.Armageddon.Gifs;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Folders;
using Worms.Cli.Resources.Local.Network;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests;

internal sealed class TestHost : IDisposable
{
    public ServiceProvider Services { get; }
    private Command RootCommand { get; }
    private Runner Runner { get; }
    public IFileSystem FileSystem { get; }
    public FakeTimeProvider Time { get; }
    public RecordingBrowserLauncher Browser { get; }
    public RecordingHttpMessageHandler Http { get; }
    public CapturingLoggerProvider Logs { get; }
    public RecordingWormsArmageddon WormsArmageddon { get; }
    public RecordingFolderOpener FolderOpener { get; }
    public StubIpAddressLookup IpAddressLookup { get; } = new();

    public TestHost(bool wormsInstalled = true, bool hostCreatesReplay = true)
    {
        FileSystem = new MockFileSystem();
        Time = new FakeTimeProvider(DateTimeOffset.UtcNow);
        Browser = new RecordingBrowserLauncher();
        Http = new RecordingHttpMessageHandler();
        Logs = new CapturingLoggerProvider();
        FolderOpener = new RecordingFolderOpener();

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

        // Wrap the registered IWormsArmageddon in a recording decorator.
        var innerDescriptor = services.Single(d => d.ServiceType == typeof(IWormsArmageddon));
        services.Remove(innerDescriptor);
        services.AddScoped<IWormsArmageddon>(sp =>
        {
            IWormsArmageddon inner;
            if (innerDescriptor.ImplementationInstance is not null)
            {
                inner = (IWormsArmageddon) innerDescriptor.ImplementationInstance;
            }
            else if (innerDescriptor.ImplementationFactory is not null)
            {
                inner = (IWormsArmageddon) innerDescriptor.ImplementationFactory(sp);
            }
            else
            {
                inner = (IWormsArmageddon) ActivatorUtilities.CreateInstance(
                    sp, innerDescriptor.ImplementationType!);
            }

            return new RecordingWormsArmageddon(inner);
        });

        services.RemoveAll<IFileSystem>();
        services.AddSingleton(FileSystem);

        services.RemoveAll<IFolderOpener>();
        services.AddSingleton<IFolderOpener>(FolderOpener);

        services.RemoveAll<IBrowserLauncher>();
        services.AddSingleton<IBrowserLauncher>(Browser);

        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(Time);

        services.RemoveAll<IIpAddressLookup>();
        services.AddSingleton<IIpAddressLookup>(IpAddressLookup);

        services.AddHttpClient(Options.DefaultName)
            .ConfigurePrimaryHttpMessageHandler(_ => Http);

        Services = services.BuildServiceProvider();
        RootCommand = CliStructure.BuildCommandLine(Services);
        Runner = Services.GetRequiredService<Runner>();

        // Resolve once so WormsArmageddon is non-null before tests use it.
        WormsArmageddon = (RecordingWormsArmageddon) Services.GetRequiredService<IWormsArmageddon>();
    }

    public Task<int> Run(params string[] args) =>
        Runner.Run(RootCommand, args, CancellationToken.None);

    public void Dispose() => Services.Dispose();
}
