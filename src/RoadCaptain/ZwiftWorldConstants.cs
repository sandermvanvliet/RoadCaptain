// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    internal struct ZwiftWorldConstants
    {
        public readonly double MetersBetweenLatitudeDegree;
        public readonly double MetersBetweenLongitudeDegree;
        public readonly double MetersBetweenLatitudeDegreeMul;
        public readonly double MetersBetweenLongitudeDegreeMul;
        public readonly double CenterLatitudeFromOrigin;
        public readonly double CenterLongitudeFromOrigin;

        public ZwiftWorldConstants(double metersBetweenLatitudeDegree, double metersBetweenLongitudeDegree, double centerLatitudeFromOrigin, double centerLongitudeFromOrigin)
        {
            MetersBetweenLatitudeDegree = metersBetweenLatitudeDegree;
            MetersBetweenLongitudeDegree = metersBetweenLongitudeDegree;
            MetersBetweenLatitudeDegreeMul = 1 / metersBetweenLatitudeDegree;
            MetersBetweenLongitudeDegreeMul = 1 / metersBetweenLongitudeDegree;
            CenterLatitudeFromOrigin = centerLatitudeFromOrigin * metersBetweenLatitudeDegree * 100;
            CenterLongitudeFromOrigin = centerLongitudeFromOrigin * metersBetweenLongitudeDegree * 100;
        }
    }
}