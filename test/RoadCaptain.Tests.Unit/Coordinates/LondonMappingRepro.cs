// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.Coordinates
{
    public class LondonMappingRepro
    {
        [Fact]
        public void LondonRegressionTest()
        {
            // Note to self: don't ever change this
            
            var a = 156612.37500000f;
            var b = -8146.51123000f;
            var c = 10324.1376953125f;
            
            var real = new GameCoordinate(a, b, c, ZwiftWorldId.London).ToTrackPoint();

            real.CoordinatesDecimal.Should().Be("N51.50263° W0.14537°");
            real.Latitude.Should().BeApproximately(51.50263f, 0.000002f);
            real.Longitude.Should().BeApproximately(-0.14537f, 0.000001f);
        }
    }
}

