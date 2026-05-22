namespace Worms.Cli.Resources.Local.Network;

public interface IIpAddressLookup
{
    IpAddressLookupResult LookupForDomain(string domain);
}

public abstract record IpAddressLookupResult
{
    public sealed record Found(string Address) : IpAddressLookupResult;
    public sealed record NotFound(string Error) : IpAddressLookupResult;
}
