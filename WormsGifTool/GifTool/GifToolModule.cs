using Autofac;
using GifTool.Gif;
using GifTool.ViewModel;
using GifTool.Worms;

namespace GifTool
{
    public class GifToolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SteamService>().As<ISteamService>().InstancePerLifetimeScope();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>().InstancePerLifetimeScope();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>().InstancePerLifetimeScope();
            builder.RegisterType<TurnParser>().As<ITurnParser>().InstancePerLifetimeScope();

            builder.RegisterType<GifEncoder>().As<IGifEncoder>().InstancePerLifetimeScope();

            builder.RegisterType<CreateGifViewModel>().InstancePerDependency();
            builder.RegisterType<SelectTurnViewModel>().InstancePerDependency();
            builder.RegisterType<SelectReplayViewModel>().InstancePerDependency();
            builder.RegisterType<MainWindowViewModel>().InstancePerDependency();
        }
    }
}
