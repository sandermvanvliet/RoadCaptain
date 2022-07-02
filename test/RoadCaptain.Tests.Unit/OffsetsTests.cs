// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
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
                        new TrackPoint(1, 1, 0, ZwiftWorldId.Watopia),
                        new TrackPoint(5, 5, 0, ZwiftWorldId.Watopia)
                    }
                    .Select(point => point.ToGameCoordinate())
                    .ToList(),
                ZwiftWorldId.Watopia);

            var inputGame = input.ToGameCoordinate();

            var scaledPoint = offsets.ScaleAndTranslate(inputGame);

            var outputGame = offsets.ReverseScaleAndTranslate(scaledPoint.X, scaledPoint.Y);

            var output = new GameCoordinate(
                    outputGame.X,
                    outputGame.Y,
                    outputGame.Altitude,
                    ZwiftWorldId.Watopia)
                .ToTrackPoint();

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
        public void RoundtripWithPadding()
        {
            var input = new TrackPoint(1.23, 4.56, 12);

            var offsets = new Offsets(
                    400,
                    400,
                    new List<TrackPoint>
                        {
                            new TrackPoint(1, 1, 0),
                            new TrackPoint(5, 5, 0)
                        }
                        .Select(point => point.ToGameCoordinate())
                        .ToList(),
                    ZwiftWorldId.Watopia)
                .Pad(15);

            var inputGame = input.ToGameCoordinate();

            var scaledPoint = offsets.ScaleAndTranslate(inputGame);

            var outputGame = offsets.ReverseScaleAndTranslate(scaledPoint.X, scaledPoint.Y);

            var output = new GameCoordinate(
                    outputGame.X,
                    outputGame.Y,
                    outputGame.Altitude,
                    ZwiftWorldId.Watopia)
                .ToTrackPoint();

            output
                .Latitude
                .Should()
                .Be(input.Latitude);

            output
                .Longitude
                .Should()
                .Be(input.Longitude);
        }
    }
}
