using Pulumi;
using Pulumi.AzureNative.App;
using Cloudflare = Pulumi.Cloudflare;

namespace Worms.Hub.Infrastructure.ContainerApps;

public static class Dns
{
    public static void Config(Config config, ManagedEnvironment managedEnvironment)
    {
        var subdomain = config.Require("subdomain");
        var domain = config.Require("domain");
        var ipAddress = managedEnvironment.StaticIp;
        var challengeTxtValue =
            managedEnvironment.CustomDomainConfiguration.Apply(x => x?.CustomDomainVerificationId ?? "");
        var zoneId = Cloudflare.GetZone.Invoke(new() { Name = domain }).Apply(x => x.Id);

        _ = new Cloudflare.Record(
            "dns-entry-ip",
            new()
            {
                ZoneId = zoneId,
                Name = subdomain,
                Content = ipAddress,
                Type = "A"
            });

        _ = new Cloudflare.Record(
            "dns-entry-txt",
            new()
            {
                ZoneId = zoneId,
                Name = "asuid." + subdomain,
                Content = challengeTxtValue,
                Type = "TXT"
            });
    }
}
