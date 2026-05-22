namespace Worms.Cli.Resources.Local.Network;

public interface IIpAddressLookup
{
    IpAddressLookupResult LookupForDomain(string domain);
}

public abstract record IpAddressLookupResult;

public sealed record IpAddressFound(string Address) : IpAddressLookupResult;

public sealed record IpAddressNotFound(string Error) : IpAddressLookupResult;
