using Worms.Cli.Resources.Local.Network;

namespace Worms.Cli.Tests.Fakes;

internal sealed class StubIpAddressLookup : IIpAddressLookup
{
    public IpAddressLookupResult Result { get; set; } = new IpAddressLookupResult.Found("10.0.0.1");

    public IpAddressLookupResult LookupForDomain(string domain) => Result;
}
