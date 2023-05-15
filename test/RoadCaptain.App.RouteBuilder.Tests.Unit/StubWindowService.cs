// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class StubWindowService : WindowService
    {
        public StubWindowService(IComponentContext componentContext,
            MonitoringEvents monitoringEvents) : base(componentContext, monitoringEvents)
        {
        }
    }
}
