using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.Coordinates
{
    public class LondonMappingRepro
    {
        //[Fact]
        public void X()
        {
            var entries = File
                .ReadAllLines(@"C:\git\temp\zwift\london-positions.txt")
                .Select(line => line.Split(" "))
                .Select(parts => new LogEntry(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[4]), ParseFloat(parts[5]), parts[7] + " " + parts[8]))
                .ToList();

            for (var index = entries.Count - 1; index >= entries.Count - 11; index--)
            {
                Console.WriteLine(entries[index]);
            }
        }

        //[Fact]
        public void ReproOne()
        {
            var gameCoordinate = new GameCoordinate(156612.38f, -8146.511f, 0, ZwiftWorldId.London);

            var trackPoint = gameCoordinate.ToTrackPoint();

            trackPoint.Latitude.Should().BeApproximately(51.502428f, 0.00001);
            trackPoint.Longitude.Should().BeApproximately(-0.145476f,  0.00001);
        }

        private float ParseFloat(string part)
        {
            return float.Parse(part, CultureInfo.InvariantCulture);
        }
    }

    public record LogEntry(float X, float Y, float Latitude, float Longitude, string CoordinatesDecimal);
}
