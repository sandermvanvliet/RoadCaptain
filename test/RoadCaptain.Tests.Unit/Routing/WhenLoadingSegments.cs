﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.SegmentBuilder;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenLoadingSegments
    {
        //[Fact]
        public void Foo()
        {
            var segmentStore = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters");

            var segments = segmentStore.LoadSegments();

            var result = new List<TrackPoint>();

            var seg = segments.Single(s => s.Id == "watopia-big-foot-hills-004-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());

            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-001-after-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-003-before");
            result.AddRange(seg.Points);
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-003-after");
            result.AddRange(seg.Points);
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-001-after-after-before-before-after-after");
            result.AddRange(seg.Points);
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-before-before");
            result.AddRange(seg.Points);
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-before-after");
            result.AddRange(seg.Points);
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-after-after-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-after-after-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-after-before-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-after-before-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-before-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-003-before-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-001-after-after-before-before-after-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-bambino-fondo-001-after-after-before-before-after-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-001-after-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-001-after-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-001-before-after");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            seg = segments.Single(s => s.Id == "watopia-big-foot-hills-001-before-before");
            result.AddRange(seg.Points.AsEnumerable().Reverse());
            
            var csvLines = result
                .Select(p =>
                    p.Latitude.ToString("0.00000", CultureInfo.InvariantCulture) + ";" +
                    p.Longitude.ToString("0.00000", CultureInfo.InvariantCulture) + ";" +
                    p.Altitude.ToString("0.00000", CultureInfo.InvariantCulture))
                .ToList();

            var csv = string.Join(Environment.NewLine, csvLines);
        }

        [Fact]
        public void ConvertLatLonToGameAndBack()
        {
            var trackPoint = new TrackPoint(-11.640437m, 166.946204m, 13.2m);

            var gamePoint = TrackPoint.LatLongToGame(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude);
            var reverted = TrackPoint.FromGameLocation(gamePoint.Latitude, gamePoint.Longitude, gamePoint.Altitude);

            reverted
                .Should()
                .BeEquivalentTo(trackPoint);
        }

        [Fact]
        public void ConvertGameToLatLon()
        {
            var gameLat = 93536.016m;
            var gameLon = 212496.77m;
            var gamePoint = new TrackPoint(gameLat, gameLon, 0);

            var reverted = TrackPoint.FromGameLocation(gamePoint.Latitude, gamePoint.Longitude, gamePoint.Altitude);

            reverted
                .CoordinatesDecimal
                .Should()
                .Be("S11.63645° E166.97237°");
        }
    }
}