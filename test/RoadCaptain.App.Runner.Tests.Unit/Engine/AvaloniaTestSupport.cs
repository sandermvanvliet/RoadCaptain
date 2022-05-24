using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using Moq;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    internal static class AvaloniaTestSupport
    {
        public static void MockServices()
        {
            var avaloniaDependencyResolver = new AvaloniaLocator();

            var windowingPlatformMock = new Mock<IWindowingPlatform>();
            windowingPlatformMock
                .Setup(_ => _.CreateWindow())
                .Returns(new Mock<IWindowImpl>().Object);
            avaloniaDependencyResolver.Bind<IWindowingPlatform>().ToConstant(windowingPlatformMock.Object);

            var assetLoaderMock = new Mock<IAssetLoader>();
            assetLoaderMock
                .Setup(_ => _.Open(It.IsAny<Uri>(), It.IsAny<Uri?>()))
                .Returns<Uri, Uri?>((uri, baseUri) => new AssetLoader(Assembly.Load(uri.Authority)).Open(uri));
            avaloniaDependencyResolver.Bind<IAssetLoader>().ToConstant(assetLoaderMock.Object);

            var iconLoaderMock = new Mock<IPlatformIconLoader>();
            iconLoaderMock.Setup(_ => _.LoadIcon(It.IsAny<Stream>()))
                .Returns<Stream>(stream =>
                {
                    try
                    {
                        return new IconImpl(new Icon(stream));
                    }
                    catch (ArgumentException)
                    {
                        return new IconImpl(new System.Drawing.Bitmap(stream));
                    }
                });
            avaloniaDependencyResolver.Bind<IPlatformIconLoader>().ToConstant(iconLoaderMock.Object);

            avaloniaDependencyResolver.Bind<ICursorFactory>().ToConstant(new Mock<ICursorFactory>().Object);
            avaloniaDependencyResolver.Bind<IPlatformRenderInterface>().ToConstant(new Mock<IPlatformRenderInterface>().Object);
            
            AvaloniaLocator.Current = avaloniaDependencyResolver;
        }
    }
}