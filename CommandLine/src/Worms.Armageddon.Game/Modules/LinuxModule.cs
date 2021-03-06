﻿using Autofac;
using Worms.Armageddon.Game.Linux;

namespace Worms.Armageddon.Game.Modules
{
    internal class LinuxModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}
