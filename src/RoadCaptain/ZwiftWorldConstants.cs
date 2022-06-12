// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    internal struct ZwiftWorldConstants
    {
        public double MetersBetweenLatitudeDegree { get; }
        public double MetersBetweenLongitudeDegree { get; }
        public double CenterLatitudeFromOrigin { get; }
        public double CenterLongitudeFromOrigin { get; }

        public ZwiftWorldConstants(double metersBetweenLatitudeDegree, double metersBetweenLongitudeDegree, double centerLatitudeFromOrigin, double centerLongitudeFromOrigin)
        {
            MetersBetweenLatitudeDegree = metersBetweenLatitudeDegree;
            MetersBetweenLongitudeDegree = metersBetweenLongitudeDegree;
            CenterLatitudeFromOrigin = centerLatitudeFromOrigin * metersBetweenLatitudeDegree * 100;
            CenterLongitudeFromOrigin = centerLongitudeFromOrigin * metersBetweenLongitudeDegree * 100;
        }
    }
}