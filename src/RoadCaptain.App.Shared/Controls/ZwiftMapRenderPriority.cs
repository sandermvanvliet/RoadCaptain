using Codenizer.Avalonia.Map;

namespace RoadCaptain.App.Shared.Controls
{
    public class ZwiftMapRenderPriority : RenderPriority
    {
        protected override int CompareCore(MapObject self, MapObject other)
        {
            if (self is WorldMap && other is not WorldMap)
            {
                return -1;
            }

            if (self is not WorldMap && other is WorldMap)
            {
                return 1;
            }

            if (self is SpawnPointSegment && other is not SpawnPointSegment)
            {
                return 1;
            }

            if (self is not SpawnPointSegment && other is SpawnPointSegment)
            {
                return -1;
            }

            if (self is RoutePath && other is not RoutePath)
            {
                return 1;
            }

            if (self is not RoutePath && other is RoutePath)
            {
                return -1;
            }
            
            return 0;
        }
    }
}