// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.Coordinates
{
    public class WhenMappingLatLongToGame
    {
        [Fact]
        public void WatopiaCenterRoundTrip()
        {
            var worldId = MapInfo.WATOPIA;
            var latitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[worldId][0];
            var longitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[worldId][1];

            var gameCoordinates = LatLngToGameCoordinates(worldId, latitude, longitude);

            var roundTrippedLatLong = gameCoordinates.ToTrackPoint();

            roundTrippedLatLong.Latitude.Should().BeApproximately(latitude, 5);
            roundTrippedLatLong.Longitude.Should().BeApproximately(longitude, 5);
        }

        [Fact]
        public void MakuriIslandsCenterRoundTrip()
        {
            var latitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[MapInfo.MAKURI][0];
            var longitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[MapInfo.MAKURI][1];

            var gameCoordinates = LatLngToGameCoordinates(MapInfo.MAKURI, latitude, longitude);

            var roundTrippedLatLong = gameCoordinates.ToTrackPoint();

            roundTrippedLatLong.Latitude.Should().BeApproximately(latitude, 5);
            roundTrippedLatLong.Longitude.Should().BeApproximately(longitude, 5);
        }

        [Fact]
        public void LondonCenterRoundTrip()
        {
            var latitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[MapInfo.LONDON][0];
            var longitude = MapInfo.LATITUDE_LONGITUDE_OFFSETS[MapInfo.LONDON][1];

            var gameCoordinates = LatLngToGameCoordinates(MapInfo.LONDON, latitude, longitude);

            var roundTrippedLatLong = gameCoordinates.ToTrackPoint();

            roundTrippedLatLong.Latitude.Should().BeApproximately(latitude, 5);
            roundTrippedLatLong.Longitude.Should().BeApproximately(longitude, 5);
        }

        //[Fact]
        // ReSharper disable once UnusedMember.Global
#pragma warning disable xUnit1013
        public void ReproOne()
#pragma warning restore xUnit1013
        {
            var gameCoordinate = new GameCoordinate(156612.38f, -8146.511f, 0, ZwiftWorldId.London);

            var trackPoint = gameCoordinate.ToTrackPoint();

            trackPoint.Latitude.Should().BeApproximately(51.502428f, 0.00001);
            trackPoint.Longitude.Should().BeApproximately(-0.145476f,  0.00001);
        }

        //[Fact]
        // ReSharper disable once UnusedMember.Global
#pragma warning disable xUnit1013
        public void ReproOneReverse()
#pragma warning restore xUnit1013
        {
            var latitude = 51.502428f;
            var longitude = -0.145476f;

            var gameCoordinates = LatLngToGameCoordinates(MapInfo.LONDON, latitude, longitude);

            gameCoordinates.X.Should().BeApproximately(156612.38f, 0.0001);
            gameCoordinates.Y.Should().BeApproximately(-8146.511f, 0.0001);

            var roundTrippedLatLong = gameCoordinates.ToTrackPoint();

            roundTrippedLatLong.Latitude.Should().BeApproximately(latitude, 5);
            roundTrippedLatLong.Longitude.Should().BeApproximately(longitude, 5);
        }

        // 
        [Fact]
        public void Y()
        {
            var gameCoordinates = new GameCoordinate(104124.30, -6180.39, 0, ZwiftWorldId.Watopia);

            var trackPoint = gameCoordinates.ToTrackPoint();

            var mapped = LatLngToGameCoordinates((int)ZwiftWorldId.Watopia, (float)trackPoint.Latitude, (float)trackPoint.Longitude);
        }


        public static GameCoordinate LatLngToGameCoordinates(int worldId, float latitude, float longitude)
        {
            var latLonOffsets = MapInfo.LATITUDE_LONGITUDE_OFFSETS[worldId];
            var latLonDegreeDistance = MapInfo.LATITUDE_LONGITUDE_DEGREE_DISTANCE[worldId];
            
            var latitudeDegreeDistance = latLonDegreeDistance[0];
            var longitudeDegreeDistance = latLonDegreeDistance[1];

            var latitudeOffset = latLonOffsets[0];
            var longitudeOffset = latLonOffsets[1];

            var latitudeIntermediate = latitudeOffset * latitudeDegreeDistance * 100.0f;
            var longitudeIntermediate = longitudeOffset * longitudeDegreeDistance * 100.0f;
            
            const float hundred = 100;

            var gameX = 0.0f;
            float gameY;

            switch (worldId)
            {
                case MapInfo.WATOPIA /* 1 */:
                case MapInfo.RICHMOND /* 2 */:
                case MapInfo.MAKURI /* 9 */:
                case MapInfo.FRANCE /* 10 */:
                case MapInfo.PARIS /* 11 */:
                case MapInfo.GRAVELMTN /* 12 */:
                    gameX = ((latitude * latitudeDegreeDistance) * hundred) - latitudeIntermediate;
                    gameY = ((longitude * longitudeDegreeDistance) * hundred) - longitudeIntermediate;
                    break;
                case MapInfo.LONDON /* 3 */:
                case MapInfo.NEWYORK /* 4 */:
                case MapInfo.INNSBRUCK /* 5 */:
                case MapInfo.BOLOGNA /* 6 */:
                case MapInfo.YORKSHIRE /* 7 */:
                case MapInfo.CRITCITY:
                    gameX = ((longitude * longitudeDegreeDistance) * hundred) - longitudeIntermediate;
                    gameY = latitudeIntermediate - ((latitude * latitudeDegreeDistance) * hundred);
                    break;
                default:
                    gameY = 0.0f;
                    break;
            }

            return new GameCoordinate(gameX, gameY, 0, (ZwiftWorldId)worldId);
        }

        private static GameCoordinate Translate(int worldId, float f, float f2)
        {
            float x = 0;
            float y = 0;
            double d = 0;

            switch (worldId)
            {
                case 1:
                    RectF rectF = MapInfo.WORLD_WATOPIA;
                    x = (f2 - rectF.Left) / rectF.Width;
                    y = ((-f) - rectF.Top) / rectF.Height;
                    break;
                case MapInfo.RICHMOND /* 2 */:
                    RectF rectF2 = MapInfo.WORLD_RICHMOND;
                    x = (f2 - rectF2.Left) / rectF2.Width;
                    y = ((-f) - rectF2.Top) / rectF2.Height;
                    break;
                case MapInfo.LONDON:
                    RectF rectF3 = MapInfo.WORLD_LONDON;
                    x = (f - rectF3.Left) / rectF3.Width;
                    y = (f2 - rectF3.Top) / rectF3.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.NEWYORK /* 4 */:
                    RectF rectF4 = MapInfo.WORLD_NEWYORK;
                    x = (f - rectF4.Left) / rectF4.Width;
                    y = (f2 - rectF4.Top) / rectF4.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.INNSBRUCK:
                    RectF rectF5 = MapInfo.WORLD_INNSBRUCK;
                    x = (f - rectF5.Left) / rectF5.Width;
                    y = (f2 - rectF5.Top) / rectF5.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.BOLOGNA /* 6 */:
                    RectF rectF6 = MapInfo.WORLD_BOLOGNA;
                    x = (f - rectF6.Left) / rectF6.Width;
                    y = (f2 - rectF6.Top) / rectF6.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.YORKSHIRE /* 7 */:
                    RectF rectF7 = MapInfo.WORLD_YORKSHIRE;
                    x = (f - rectF7.Left) / rectF7.Width;
                    y = (f2 - rectF7.Top) / rectF7.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.CRITCITY:
                    RectF rectF8 = MapInfo.WORLD_CRITCITY;
                    x = (f - rectF8.Left) / rectF8.Width;
                    y = (f2 - rectF8.Top) / rectF8.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.MAKURI /* 9 */:
                    RectF rectF9 = MapInfo.WORLD_MAKURI;
                    x = (f - rectF9.Left) / rectF9.Width;
                    y = (f2 - rectF9.Top) / rectF9.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.FRANCE /* 10 */:
                    RectF rectF10 = MapInfo.WORLD_FRANCE;
                    x = ((f * 1.3325f) - rectF10.Left) / rectF10.Width;
                    y = ((f2 * 1.3335f) - rectF10.Top) / rectF10.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.PARIS /* 11 */:
                    RectF rectF11 = MapInfo.WORLD_PARIS;
                    x = ((f * 0.5f) - rectF11.Left) / rectF11.Width;
                    y = ((f2 * 0.5f) - rectF11.Top) / rectF11.Height;
                    d -= 1.5707963267948966d;
                    break;
                case MapInfo.GRAVELMTN /* 12 */:
                    RectF rectF12 = MapInfo.WORLD_GRAVELMTN;
                    x = (f - rectF12.Left) / rectF12.Width;
                    y = (f2 - rectF12.Top) / rectF12.Height;
                    d -= 1.5707963267948966d;
                    break;
            }

            return new GameCoordinate(x, y, 0, (ZwiftWorldId)worldId);
        }
    }
}
