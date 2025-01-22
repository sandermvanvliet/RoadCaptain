// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Platform;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class WorldMap : MapObject
    {
        private readonly SKImage? _image;

        public WorldMap(string worldId)
        {
            Name = $"worldMap-{worldId}";
            
            var stream = AssetLoader.Open(new Uri($"avares://RoadCaptain.App.Shared/Assets/map-{worldId}.png"));
            _image = SKImage.FromEncodedData(stream);

            Bounds = new SKRect(0, 0, _image.Width, _image.Height);
        }

        protected override void RenderCore(SKCanvas canvas)
        {
            if (_image != null)
            {
                canvas.DrawImage(_image, Bounds);
            }
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public override bool IsSelectable { get; set; } = false;
        public override bool IsVisible { get; set; } = true;
    }
}
