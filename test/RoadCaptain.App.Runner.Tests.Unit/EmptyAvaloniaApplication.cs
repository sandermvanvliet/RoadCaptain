// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<RoadCaptain.App.Runner.Tests.Unit.EmptyAvaloniaApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

namespace RoadCaptain.App.Runner.Tests.Unit
{
    public class EmptyAvaloniaApplication : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
