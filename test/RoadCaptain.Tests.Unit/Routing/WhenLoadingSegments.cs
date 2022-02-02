using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        //[Fact]
        public void ExtrapolateSegment()
        {
            var points = new List<TrackPoint>
            {
                //new TrackPoint(-73747.22m, 47586.45m, 13.20m),
                //new TrackPoint(-72566.91m, 48902.76m, 13.40m),
                //new TrackPoint(-71168.03m, 49964.66m, 13.40m),
                //new TrackPoint(-69638.01m, 50927.01m, 13.20m),
                //new TrackPoint(-68228.20m, 51845.11m, 12.80m),
                new TrackPoint(-66709.10m, 52630.48m, 12.40m),
                new TrackPoint(-65069.79m, 53360.54m, 12.00m),
                new TrackPoint(-63310.26m, 54057.41m, 11.60m),
                new TrackPoint(-61670.95m, 54765.34m, 11.00m),
                new TrackPoint(-60020.71m, 55495.40m, 10.40m),
                new TrackPoint(-58381.39m, 56203.33m, 10.00m),
                new TrackPoint(-56742.08m, 56977.64m, 9.40m),
                new TrackPoint(-54982.55m, 57840.43m, 8.80m),
                new TrackPoint(-53343.24m, 58813.84m, 8.40m),
                new TrackPoint(-51813.21m, 59864.68m, 7.80m),
                new TrackPoint(-50414.33m, 61037.20m, 7.00m),
                new TrackPoint(-49004.52m, 62430.94m, 6.40m),
                new TrackPoint(-48064.65m, 64056.98m, 5.80m),
                new TrackPoint(-47015.49m, 65694.08m, 5.40m),
                new TrackPoint(-46184.91m, 67419.67m, 4.80m),
                new TrackPoint(-45365.25m, 69211.62m, 4.40m),
                new TrackPoint(-44665.81m, 70992.52m, 4.00m),
                new TrackPoint(-43966.37m, 72806.60m, 3.60m),
                new TrackPoint(-43496.43m, 74609.62m, 3.20m),
                new TrackPoint(-42906.28m, 76390.52m, 2.80m),
                new TrackPoint(-42206.84m, 78160.35m, 2.60m),
                new TrackPoint(-41387.18m, 79908.07m, 2.20m),
                new TrackPoint(-40676.81m, 81655.78m, 2.00m),
                new TrackPoint(-39627.65m, 83303.94m, 1.80m),
                new TrackPoint(-38808.00m, 84929.97m, 1.60m),
                new TrackPoint(-37868.13m, 86467.52m, 1.40m),
                new TrackPoint(-36687.82m, 88038.25m, 1.40m),
                new TrackPoint(-35638.66m, 89553.67m, 1.40m),
                new TrackPoint(-34698.79m, 91046.97m, 1.40m),
                new TrackPoint(-33529.41m, 92473.90m, 1.40m),
                new TrackPoint(-32349.11m, 93845.52m, 1.40m),
                new TrackPoint(-31179.73m, 95128.65m, 1.60m),
                new TrackPoint(-29890.14m, 96378.60m, 1.60m),
                new TrackPoint(-28480.33m, 97506.87m, 1.60m),
                new TrackPoint(-26961.23m, 98513.46m, 1.60m),
                new TrackPoint(-25551.42m, 99398.38m, 1.60m),
                new TrackPoint(-24032.33m, 100205.87m, 1.60m),
                new TrackPoint(-22502.30m, 100980.17m, 1.60m),
                new TrackPoint(-20862.99m, 101699.16m, 1.60m),
                new TrackPoint(-19343.89m, 102329.67m, 1.60m),
                new TrackPoint(-17693.65m, 102915.93m, 1.60m),
                new TrackPoint(-16054.34m, 103391.57m, 1.60m),
                new TrackPoint(-14294.81m, 103789.78m, 1.60m),
                new TrackPoint(-12535.28m, 104044.20m, 1.60m),
                new TrackPoint(-10775.75m, 104165.87m, 1.60m),
                new TrackPoint(-9136.44m, 104121.63m, 1.60m),
                new TrackPoint(-7497.12m, 103834.03m, 1.60m),
                new TrackPoint(-5857.81m, 103347.32m, 1.60m),
                new TrackPoint(-4327.79m, 102550.90m, 1.60m),
                //new TrackPoint(-3038.19m, 101500.06m, 1.60m),
            };

            points = points.Select(p => p).Reverse().ToList();

            var totalCount = 410;

            decimal countPerPoint = (decimal)totalCount / points.Count;
            var result = new List<TrackPoint>();

            for (var index = 1; index < points.Count; index++)
            {
                var previousPoint = points[index - 1];
                var point = points[index];

                decimal latIncrement;
                decimal lonIncrement;
                decimal altIncrement;
                var latDiff = Math.Abs(previousPoint.Latitude - point.Latitude);
                var lonDiff = Math.Abs(previousPoint.Longitude - point.Longitude);
                var altDiff = Math.Abs(previousPoint.Altitude - point.Altitude);

                if (previousPoint.Latitude < point.Latitude)
                {
                    latIncrement = latDiff / countPerPoint;
                }
                else
                {
                    latIncrement = -(latDiff / countPerPoint);
                }

                if (previousPoint.Longitude < point.Longitude)
                {
                    lonIncrement = lonDiff / countPerPoint;
                }
                else
                {
                    lonIncrement = -(lonDiff / countPerPoint);
                }

                if (previousPoint.Altitude < point.Altitude)
                {
                    altIncrement = altDiff / countPerPoint;
                }
                else
                {
                    altIncrement = -(altDiff / countPerPoint);
                }

                for (var e = 0; e < countPerPoint; e++)
                {
                    result.Add(new TrackPoint(
                        previousPoint.Latitude + (e * latIncrement),
                        previousPoint.Longitude + (e * lonIncrement),
                        previousPoint.Altitude + (e * altIncrement)));
                }
            }

            var csvLines = result
                .Select(p =>
                    p.Latitude.ToString("0.00000", CultureInfo.CurrentCulture) + ";" +
                    p.Longitude.ToString("0.00000", CultureInfo.CurrentCulture) + ";" +
                    p.Altitude.ToString("0.00000", CultureInfo.CurrentCulture))
                .ToList();

            var csv = string.Join(Environment.NewLine, csvLines);
        }

        //[Fact]
        public void Dwim()
        {
            var segmentStore = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters");

            var segments = segmentStore.LoadSegments();
            
            var scaleX = 0.999818958m;
            var scaleY = 1.007006443m;

            var gameTrack = File.ReadAllLines(@"c:\git\temp\zwift\position-alt.csv")
                .Select(line =>
                {
                    var parts = line.Split(';');

                    var sourceX = decimal.Parse(parts[0], CultureInfo.InvariantCulture) * scaleX;
                    var sourceY = decimal.Parse(parts[1], CultureInfo.InvariantCulture) * scaleY;

                    return new TrackPoint(sourceX, sourceY, 0);
                })
                .ToList();

            foreach (var point in gameTrack)
            {
                var match = segments.Where(s => s.Contains(point, true)).ToList();

                if (match.Any())
                {
                    Debugger.Break();
                }
            }
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
