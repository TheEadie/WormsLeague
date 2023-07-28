using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Worms.Gateway;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((_, builder) => builder.AddConsole())
            .ConfigureAppConfiguration((_, config) => config.AddEnvironmentVariables("WORMS_"))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
