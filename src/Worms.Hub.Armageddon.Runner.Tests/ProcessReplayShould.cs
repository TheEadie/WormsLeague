using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Queues;

namespace Worms.Hub.Armageddon.Runner.Tests;

// Prerequisite: WA game files must be at /home/eadie/games/worms and sample.WAGame must be in sample-data/replays/
// Run with: dotnet test --filter Category=Integration
// The test builds the Docker image and starts/stops services automatically.
[TestFixture]
[Category("Integration")]
internal sealed class ProcessReplayShould
{
    private const string ReplayFileName = "sample.WAGame";

    private const string AzuriteConnectionString =
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;"
        + "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;"
        + "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
        + "QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

    private string _repoRoot = null!;
    private string _replayFolder = null!;
    private IMessageQueue<ReplayToProcessMessage> _inputQueue = null!;
    private IMessageQueue<ReplayToUpdateMessage> _outputQueue = null!;

    [OneTimeSetUp]
    public async Task StartServices()
    {
        _repoRoot = FindRepoRoot();
        _replayFolder = Path.Combine(_repoRoot, "sample-data", "replays");

        await TestContext.Progress.WriteLineAsync("Building hub-wa-runner image...");
        await RunDockerCompose(_repoRoot, "build hub-wa-runner");
        await TestContext.Progress.WriteLineAsync("Starting services...");
        await RunDockerCompose(_repoRoot, "up -d azure-storage hub-wa-runner");
        await TestContext.Progress.WriteLineAsync("Waiting for Azurite...");
        await WaitForAzurite();
        await TestContext.Progress.WriteLineAsync("Services ready.");

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?> { ["ConnectionStrings:Storage"] = AzuriteConnectionString })
                    .Build())
            .AddQueueServices()
            .BuildServiceProvider();

        _inputQueue = services.GetRequiredService<IMessageQueue<ReplayToProcessMessage>>();
        _outputQueue = services.GetRequiredService<IMessageQueue<ReplayToUpdateMessage>>();
    }

    [SetUp]
    public void CleanUpGeneratedFiles()
    {
        var replayName = Path.GetFileNameWithoutExtension(ReplayFileName);

        var logFile = Path.Combine(_replayFolder, replayName + ".log");
        if (File.Exists(logFile))
        {
            File.Delete(logFile);
        }

        // Clean up any generated GIF files
        foreach (var gif in Directory.GetFiles(_replayFolder, "*.gif"))
        {
            File.Delete(gif);
        }
    }

    [TearDown]
    public void CleanUpGeneratedFilesAfter() => CleanUpGeneratedFiles();

    [OneTimeTearDown]
    public async Task StopServices()
    {
        await RunDockerCompose(_repoRoot, "stop hub-wa-runner azure-storage");
    }

    [Test]
    public async Task ProduceALogFileFromAReplay()
    {
        var logFilePath = Path.Combine(_replayFolder, Path.GetFileNameWithoutExtension(ReplayFileName) + ".log");

        File.Exists(logFilePath).ShouldBeFalse("Log file should not exist before the test starts");

        await TestContext.Progress.WriteLineAsync($"Sending message for {ReplayFileName}...");
        await _inputQueue.EnqueueMessage(new ReplayToProcessMessage(ReplayFileName));

        await TestContext.Progress.WriteLineAsync("Waiting for log file with content (up to 2 minutes)...");
        await PollUntil(() => File.Exists(logFilePath) && new FileInfo(logFilePath).Length > 0, TimeSpan.FromMinutes(2));
        await TestContext.Progress.WriteLineAsync("Log file found.");

        var logContent = await File.ReadAllTextAsync(logFilePath);
        logContent.ShouldContain("Game ID:");
        logContent.ShouldContain("Game Engine Version:");
        logContent.ShouldContain("File Format Version:");
        logContent.ShouldContain("Exported with Version:");
        logContent.ShouldContain("Game Started at");

        await TestContext.Progress.WriteLineAsync("Waiting for output queue message (GIF generation takes ~1 min per turn via Wine)...");
        await PollUntil(async () => await _outputQueue.HasPendingMessage(), TimeSpan.FromMinutes(20));

        var (outputMessage, _, _) = await _outputQueue.DequeueMessage();
        outputMessage.ShouldNotBeNull();
        outputMessage.ReplayFileName.ShouldBe(ReplayFileName);

        // Verify GIFs were generated
        outputMessage.TurnGifs.ShouldNotBeNull();
        outputMessage.TurnGifs.ShouldNotBeEmpty();

        await TestContext.Progress.WriteLineAsync($"Generated {outputMessage.TurnGifs.Count} turn GIFs.");

        foreach (var turnGif in outputMessage.TurnGifs)
        {
            turnGif.TurnNumber.ShouldBeGreaterThan(0);
            turnGif.GifFileName.ShouldNotBeNullOrWhiteSpace();

            var gifPath = Path.Combine(_replayFolder, turnGif.GifFileName);
            File.Exists(gifPath).ShouldBeTrue($"GIF file should exist: {gifPath}");

            var gifSize = new FileInfo(gifPath).Length;
            gifSize.ShouldBeGreaterThan(1024, $"GIF for turn {turnGif.TurnNumber} should be larger than 1KB (was {gifSize} bytes)");

            await TestContext.Progress.WriteLineAsync(
                $"Turn {turnGif.TurnNumber}: {turnGif.GifFileName} ({gifSize / 1024}KB)");
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docker-compose.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find repo root (docker-compose.yaml not found)");
    }

    private static async Task RunDockerCompose(string workingDirectory, string arguments)
    {
        using var process = Process.Start(
            new ProcessStartInfo("docker", $"compose {arguments}")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            })!;

        var stdoutReader = process.StandardOutput;
        var stderrReader = process.StandardError;

        var stdout = Task.Run(async () =>
        {
            while (await stdoutReader.ReadLineAsync() is { } line)
            {
                await TestContext.Progress.WriteLineAsync(line);
            }
        });
        var stderr = Task.Run(async () =>
        {
            while (await stderrReader.ReadLineAsync() is { } line)
            {
                await TestContext.Progress.WriteLineAsync(line);
            }
        });

        await process.WaitForExitAsync();
        await Task.WhenAll(stdout, stderr);
    }

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    private static async Task WaitForAzurite()
    {
        var azuriteUri = new Uri("http://127.0.0.1:10001/devstoreaccount1");
        var deadline = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _ = await HttpClient.GetAsync(azuriteUri);
                return;
            }
            catch (HttpRequestException)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (TaskCanceledException)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new TimeoutException("Azurite did not become reachable within 30 seconds");
    }

    private static async Task PollUntil(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"Condition not met within {timeout.TotalSeconds}s timeout");
    }

    private static async Task PollUntil(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"Condition not met within {timeout.TotalSeconds}s timeout");
    }
}
