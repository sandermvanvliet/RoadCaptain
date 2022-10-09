using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace RoadCaptain.Tests.Unit
{
    public class TrackPointCodeGeneration
    {
        // This "test" is used to generate the TrackPoint.FromGameLocation method with
        // the performance optimizations (unrolling the switch, using multiplication instead
        // of divisions, etc.
        // Because that code is highly repetitive and pretty much unreadable this bit of code
        // generates the contents of that method for all Zwift worlds.

        //[Fact]
        public void TrackPointInlineCodeGeneration()
        {
            var builder = new StringBuilder();
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            foreach (var x in TrackPoint.ZwiftWorlds)
            {
                builder.AppendLine(@$"
            if (worldId == ZwiftWorldId.{x.Key})
            {{
                var latitudeAsCentimetersFromOrigin = latitudeOffsetCentimeters + {x.Value.CenterLatitudeFromOrigin};
                var latitude = latitudeAsCentimetersFromOrigin * {x.Value.MetersBetweenLatitudeDegreeMul} * 0.01;

                var longitudeAsCentimetersFromOrigin = longitudeOffsetCentimeters + {x.Value.CenterLongitudeFromOrigin};
                var longitude = longitudeAsCentimetersFromOrigin * {x.Value.MetersBetweenLongitudeDegreeMul} * 0.01;

                return new TrackPoint(latitude, longitude, altitude);
            }}");
            }

            Debug.WriteLine(builder.ToString());

            throw new Exception("BANG");
        }
    }
}