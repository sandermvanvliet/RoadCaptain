// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RoadCaptain.Tests.Unit.Coordinates
{
    public class LondonMappingRepro
    {
        [Fact]
        public void ThisIsTheRightMapping()
        {
            var inputY = -53141.75781f;
            var inputX = 315245.5f;
            var expectedLat = 51.50649f;
            var expectedLon = -0.12252f;

            var trackPoint = Calculate(-inputY, inputX);
            
            trackPoint.Latitude.Should().BeApproximately(expectedLat, 0.00001);
            trackPoint.Longitude.Should().BeApproximately(expectedLon, 0.00001);
            trackPoint.CoordinatesDecimal.Should().Be("N51.50648° W0.12252°");

            var trackPointReal = new GameCoordinate(inputX, inputY, 0, ZwiftWorldId.London).ToTrackPoint();

            trackPointReal.Should().Be(trackPoint);
        }

        private static TrackPoint Calculate(float a, float b)
        {
            // London flips inputs
            var latitudeAsCentimetersFromOrigin = a + 572999216.4279556;
            var latitude = latitudeAsCentimetersFromOrigin * 8.988093472576876E-06 * 0.01;

            var longitudeAsCentimetersFromOrigin = b + -1165514.8567129374;
            var longitude = longitudeAsCentimetersFromOrigin * 1.4409163767062611E-05 * 0.01;
                
            return new TrackPoint(latitude, longitude, 0, ZwiftWorldId.London);
        }
    }
}

