using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;

namespace Worms.Hub.Storage.Tests;

[SetUpFixture]
internal sealed class ComposeDatabase
{
    private const string ConnectionString =
        "Host=127.0.0.1;Port=5432;Database=worms;Username=worms;Password=worms;Timeout=5";

    private static readonly TimeSpan ComposeTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(30);

    public static ServiceProvider CreateHubStorageServices() =>
        new ServiceCollection()
            .AddSingleton<IConfiguration>(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:Database"] = ConnectionString
                        })
                    .Build())
            .AddHubStorageServices()
            .BuildServiceProvider();

    [OneTimeSetUp]
    public async Task StartDatabaseWithMigrations()
    {
        var repoRoot = FindRepoRoot();

        await TestContext.Progress.WriteLineAsync("Starting compose database service...");
        await RunDockerCompose(repoRoot, "up --detach --wait database");
        await TestContext.Progress.WriteLineAsync("Applying migrations via flyway-init...");
        await RunDockerCompose(repoRoot, "run --rm flyway-init");
        await TestContext.Progress.WriteLineAsync("Waiting for database connection...");
        await WaitForConnection();
        await TestContext.Progress.WriteLineAsync("Database ready.");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docker-compose.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find repo root (docker-compose.yaml not found)");
    }

    private static async Task RunDockerCompose(string workingDirectory, string arguments)
    {
        var startInfo = new ProcessStartInfo("docker", $"compose {arguments}")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = StartDocker(startInfo, arguments);
        var output = new StringBuilder();
        var stdout = Relay(process.StandardOutput, output);
        var stderr = Relay(process.StandardError, output);

        using var timeout = new CancellationTokenSource(ComposeTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            string capturedOutput;
            lock (output)
            {
                capturedOutput = output.ToString();
            }

            throw new TimeoutException(
                $"'docker compose {arguments}' did not complete within {ComposeTimeout.TotalSeconds}s.\n{capturedOutput}");
        }

        await Task.WhenAll(stdout, stderr);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'docker compose {arguments}' failed with exit code {process.ExitCode}. "
                + $"Is Docker running?\n{output}");
        }
    }

    private static Process StartDocker(ProcessStartInfo startInfo, string arguments)
    {
        try
        {
            return Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Failed to start 'docker compose {arguments}'");
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException(
                $"Docker is not available: could not run 'docker compose {arguments}' ({exception.Message})",
                exception);
        }
    }

    private static async Task Relay(StreamReader reader, StringBuilder output)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            lock (output)
            {
                _ = output.AppendLine(line);
            }

            await TestContext.Progress.WriteLineAsync(line);
        }
    }

    private static async Task WaitForConnection()
    {
        var deadline = DateTime.UtcNow.Add(ConnectionTimeout);
        NpgsqlException? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch (NpgsqlException exception)
            {
                lastError = exception;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new TimeoutException(
            $"Database at 127.0.0.1:5432 did not accept connections within {ConnectionTimeout.TotalSeconds}s",
            lastError);
    }
}
