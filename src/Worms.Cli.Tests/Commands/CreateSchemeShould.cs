using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files.Schemes.Text;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class CreateSchemeShould
{
    private static readonly Lazy<string> ValidSchemeText = new(() =>
    {
        using var host = new TestHost();
        var writer = host.Services.GetRequiredService<ISchemeTextWriter>();
        using var output = new StringWriter();
        writer.Write(new Syroot.Worms.Armageddon.Scheme(), output);
        return output.ToString();
    });

    [Test]
    public async Task CreateSchemeFromRandomWhenWormsIsInstalled()
    {
        using var host = new TestHost();
        var folder = host.WormsArmageddon.FindInstallation().SchemesFolder;

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "scheme", "myscheme", "--random");

        exitCode.ShouldBe(0);
        var expectedPath = Path.Combine(folder, "myscheme.wsc");
        console.Output.ToString().ShouldContain(expectedPath);
        host.FileSystem.File.Exists(expectedPath).ShouldBeTrue();
    }

    [Test]
    public async Task CreateSchemeFromFile()
    {
        using var host = new TestHost();
        var folder = host.WormsArmageddon.FindInstallation().SchemesFolder;
        const string definitionPath = "/tmp/scheme.txt";
        host.FileSystem.Directory.CreateDirectory("/tmp");
        await host.FileSystem.File.WriteAllTextAsync(definitionPath, ValidSchemeText.Value);

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "scheme", "myscheme", "--file", definitionPath);

        exitCode.ShouldBe(0);
        var expectedPath = Path.Combine(folder, "myscheme.wsc");
        console.Output.ToString().ShouldContain(expectedPath);
        host.FileSystem.File.Exists(expectedPath).ShouldBeTrue();
    }

    [Test]
    public async Task ReturnNonZeroWhenWormsNotInstalledAndNoResourceFolder()
    {
        using var host = new TestHost(wormsInstalled: false);

        var exitCode = await host.Run("create", "scheme", "myscheme", "--random");

        exitCode.ShouldBe(1);
    }

    [Test]
    public async Task CreateSchemeInResourceFolderWhenWormsNotInstalled()
    {
        using var host = new TestHost(wormsInstalled: false);
        const string customFolder = "/tmp/custom-schemes";

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "scheme", "myscheme", "--random", "--resource-folder", customFolder);

        exitCode.ShouldBe(0);
        var expectedPath = Path.Combine(customFolder, "myscheme.wsc");
        console.Output.ToString().ShouldContain(expectedPath);
        host.FileSystem.File.Exists(expectedPath).ShouldBeTrue();
    }
}
