using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Worms.Cli.Resources.Local.Network;

internal sealed class NetworkInterfaceIpAddressLookup : IIpAddressLookup
{
    public IpAddressLookupResult LookupForDomain(string domain)
    {
        var adapter = Array.Find(
            NetworkInterface.GetAllNetworkInterfaces(),
            a => a.GetIPProperties().DnsSuffix == domain
                 && a.OperationalStatus == OperationalStatus.Up);

        if (adapter is null)
        {
            return new IpAddressLookupResult.NotFound($"No network adapter found for domain: {domain}");
        }

        var ipv4 = adapter.GetIPProperties()
            .UnicastAddresses.FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork)
            ?.Address.ToString();

        return ipv4 is null
            ? new IpAddressLookupResult.NotFound($"No IPv4 address found for domain: {domain}")
            : new IpAddressLookupResult.Found(ipv4);
    }
}
