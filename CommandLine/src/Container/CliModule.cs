using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Cli;
using Worms.Cli.PackageManagers;
using Worms.Configuration;
using Worms.League;
using Worms.Logging;
using Worms.Resources.Schemes;
using Worms.Slack;
using Worms.WormsArmageddon.Schemes.WscFiles;

namespace Worms.Container
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOSModules(builder);

            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Config
            builder.RegisterType<ConfigManager>().As<IConfigManager>();

            // CLI
            builder.RegisterType<GitHubReleasePackageManager>();
            builder.RegisterType<CliUpdater>();
            builder.RegisterType<CliInfoRetriever>();

            // Annoucer
            builder.RegisterType<SlackAnnouncer>().As<ISlackAnnouncer>();

            // League
            builder.RegisterType<LeagueUpdater>();

            // Schemes
            builder.RegisterType<DefaultSchemesPrinter>().As<IResourcePrinter<SchemeResource>>();
            builder.RegisterType<LocalSchemesRetriever>().As<ISchemesRetriever>();
            builder.RegisterType<WscReader>();
        }

        private static void RegisterOSModules(ContainerBuilder builder)
        {
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.RegisterModule<WindowsCliModule>();
            }

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.RegisterModule<LinuxCliModule>();
            }
        }
    }
}
