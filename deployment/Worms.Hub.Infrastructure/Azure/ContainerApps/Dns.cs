using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.Cloudflare.Inputs;
using Cloudflare = Pulumi.Cloudflare;

namespace Worms.Hub.Infrastructure.Azure.ContainerApps;

internal static class Dns
{
    public static void Config(Config config, ManagedEnvironment managedEnvironment)
    {
        var subdomain = config.Require("subdomain");
        var domain = config.Require("domain");
        var ipAddress = managedEnvironment.StaticIp;
        var challengeTxtValue =
            managedEnvironment.CustomDomainConfiguration.Apply(x => x?.CustomDomainVerificationId ?? "");
        var zoneId = Cloudflare.GetZone
            .Invoke(new Cloudflare.GetZoneInvokeArgs { Filter = new GetZoneFilterInputArgs { Name = domain, Match = "all"} })
            .Apply(x => x.Id);

        _ = new Cloudflare.DnsRecord(
            "dns-entry-ip",
            new Cloudflare.DnsRecordArgs
            {
                ZoneId = zoneId,
                Name = subdomain,
                Content = ipAddress,
                Type = "A",
                Ttl = 1
            });

        _ = new Cloudflare.DnsRecord(
            "dns-entry-txt",
            new Cloudflare.DnsRecordArgs
            {
                ZoneId = zoneId,
                Name = "asuid." + subdomain,
                Content = challengeTxtValue,
                Type = "TXT",
                Ttl = 1
            });
    }
}
