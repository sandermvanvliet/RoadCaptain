using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<RoadCaptain.App.Shared.Tests.Unit.EmptyAvaloniaApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

namespace RoadCaptain.App.Shared.Tests.Unit
{
    public class EmptyAvaloniaApplication : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}