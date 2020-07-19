using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Cli;
using Worms.Cli.PackageManagers;
using Worms.Configuration;
using Worms.League;
using Worms.Logging;
using Worms.Resources.Schemes;
using Worms.Resources.Schemes.Binary;
using Worms.Resources.Schemes.Text;
using Worms.Slack;

namespace Worms.Container
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOsModules(builder);

            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Config
            builder.RegisterType<ConfigManager>().As<IConfigManager>();

            // CLI
            builder.RegisterType<GitHubReleasePackageManager>();
            builder.RegisterType<CliUpdater>();
            builder.RegisterType<CliInfoRetriever>();

            // Announcer
            builder.RegisterType<SlackAnnouncer>().As<ISlackAnnouncer>();

            // League
            builder.RegisterType<LeagueUpdater>();

            // Schemes
            builder.RegisterType<SchemeTextPrinter>().As<IResourcePrinter<SchemeResource>>();
            builder.RegisterType<LocalSchemesRetriever>().As<ISchemesRetriever>();
            builder.RegisterType<WscReader>().As<IWscReader>();
            builder.RegisterType<WscWriter>().As<IWscWriter>();
            builder.RegisterType<SchemeTextReader>().As<ISchemeTextReader>();
        }

        private static void RegisterOsModules(ContainerBuilder builder)
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
