using System.Diagnostics.CodeAnalysis;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class SchemeFilesAuthShould
{
    private static readonly Uri SchemeRedgateUri = new("api/v1/files/schemes/redgate", UriKind.Relative);

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp() => _host = new GatewayTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task Return401WhenNoTokenIsSupplied()
    {
        using var client = _host.CreateClient();
        var response = await client.GetAsync(SchemeRedgateUri);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenTokenLacksAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithoutAccessRole());
        var response = await client.GetAsync(SchemeRedgateUri);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenTokenHasAccessRole()
    {
        await File.WriteAllTextAsync(Path.Combine(_host.SchemesFolder, "redgate-version.txt"), "1.2.3");
        await File.WriteAllBytesAsync(Path.Combine(_host.SchemesFolder, "redgate.wsc"), [0x00]);

        using var client = _host.CreateClient(TestJwt.WithAccessRole());
        var response = await client.GetAsync(SchemeRedgateUri);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
