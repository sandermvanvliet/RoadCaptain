// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Avalonia.Platform;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    internal class IconImpl : IWindowIconImpl
    {
        private readonly Bitmap? _bitmap;
        private readonly Icon? _icon;

        public IconImpl(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public IconImpl(Icon icon)
        {
            _icon = icon;
        }

        public IntPtr HIcon => _icon?.Handle ?? _bitmap?.GetHicon() ?? IntPtr.Zero;

        public void Save(Stream outputStream)
        {
            if (_icon != null)
            {
                _icon.Save(outputStream);
            }
            else if(_bitmap != null)
            {
                _bitmap.Save(outputStream, ImageFormat.Png);
            }
            else
            {
                throw new InvalidOperationException("No icon or bitmap available to save");
            }
        }
    }
}
