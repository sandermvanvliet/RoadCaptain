// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.App.Runner.Tests.Unit.Engine;
using RoadCaptain.App.Runner.Tests.Unit.ViewModels;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit
{
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Replace some services
            builder.RegisterType<StubWindowService>().SingleInstance().AsSelf().As<IWindowService>();
            builder.RegisterType<StubRouteStore>().AsImplementedInterfaces();
            builder.RegisterType<StubMessageReceiver>().AsImplementedInterfaces();
            builder.RegisterType<InMemoryZwiftGameConnection>().As<IZwiftGameConnection>().SingleInstance();
            builder.RegisterType<DummyUserPreferences>().As<IUserPreferences>().SingleInstance();
        }
    }
}
