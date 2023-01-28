// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public class CapturedWindowLocation
    {
        public CapturedWindowLocation(int x, int y, bool isMaximized, int? width, int? height)
        {
            X = x;
            Y = y;
            IsMaximized = isMaximized;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public bool IsMaximized { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
