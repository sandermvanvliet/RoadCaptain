using System;
using Avalonia;
using Avalonia.Platform;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Views
{
    internal class WorldMap : MapObject
    {
        private readonly SKImage _image;

        public WorldMap(string worldId)
        {
            Name = $"worldMap-{worldId}";
            
            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var stream = assetLoader.Open(new Uri($"avares://RoadCaptain.App.Shared/Assets/map-{worldId}.png"));
            _image = SKImage.FromEncodedData(stream);

            Bounds = new SKRect(0, 0, _image.Width, _image.Height);
        }

        public override void Render(SKCanvas canvas)
        {
            canvas.DrawImage(_image, Bounds);
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
    }
}