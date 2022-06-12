// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using RoadCaptain.App.RouteBuilder.Models;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class OffsetsTests
    {
        [Fact]
        public void Roundtrip()
        {
            var input = new TrackPoint(1.23, 4.56, 12);

            var offsets = new Offsets(
                400, 
                400,
                new List<TrackPoint>
            {
                new TrackPoint(1, 1, 0),
                new TrackPoint(5, 5, 0)
            });

            var scaledPoint = offsets.ScaleAndTranslate(input);
            var output = offsets.ReverseScaleAndTranslate(scaledPoint.X, scaledPoint.Y);

            output
                .Latitude
                .Should()
                .Be(input.Latitude);

            output
                .Longitude
                .Should()
                .Be(input.Longitude);
        }

        [Fact]
        public void Foo()
        {
            var x = new ZwiftWorldConstants(110614.71d, 109287.52d, -11.644904f, 166.95293);

            Debug.WriteLine($"Center lat: {x.CenterLatitudeFromOrigin}");
            Debug.WriteLine($"Center lat: {x.CenterLatitudeFromOrigin}");
            Debug.WriteLine($"meters lat: {x.MetersBetweenLatitudeDegreeMul}");
            Debug.WriteLine($"meters lon: {x.MetersBetweenLongitudeDegreeMul}");

            Debugger.Break();
        }
    }
}
