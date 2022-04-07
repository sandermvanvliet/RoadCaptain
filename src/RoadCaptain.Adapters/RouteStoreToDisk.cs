// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RouteStoreToDisk : IRouteStore
    {
        private readonly ISegmentStore _segmentStore;

        private static readonly JsonSerializerSettings RouteSerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };

        private List<Segment> _segments;

        public RouteStoreToDisk(ISegmentStore segmentStore)
        {
            _segmentStore = segmentStore;
        }

        public PlannedRoute LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return JsonConvert.DeserializeObject<PlannedRoute>(
                File.ReadAllText(path),
                RouteSerializationSettings);
        }

        public void Store(PlannedRoute route, string path)
        {
            var serialized = path.EndsWith(".gpx", StringComparison.InvariantCultureIgnoreCase)
                ? SerializeAsGpx(route)
                : SerializeAsJson(route);

            File.WriteAllText(path, serialized);
        }

        private string SerializeAsGpx(PlannedRoute route)
        {
            var trackSegments = string.Join(
                Environment.NewLine,
                route.RouteSegmentSequence.Select(seg => "<trkseg>" + SegmentAsTrackPointList (seg) + "</trkseg>"));
            
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<gpx creator=\"RoadCaptain:RouteBuilder\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>RoadCaptain route starting at {route.ZwiftRouteName}</name>" +
                   $"<desc>RoadCaptain route starting at {route.ZwiftRouteName}</desc>" +
                   "<type>RoadCaptain route</type>" +
                   trackSegments +
                   "</trk>" +
                   "</gpx>";
        }

        private string SegmentAsTrackPointList(SegmentSequence segment)
        {
            var actualSegment = GetSegmentById(segment.SegmentId);

            return string.Join(
                Environment.NewLine,
                actualSegment
                    .Points
                    .Select(x =>
                        $"<trkpt lat=\"{x.Latitude.ToString(CultureInfo.InvariantCulture)}\" lon=\"{x.Longitude.ToString(CultureInfo.InvariantCulture)}\"><ele>{x.Altitude.ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                    .ToList());

        }

        private Segment GetSegmentById(string segmentId)
        {
            if (_segments == null)
            {
                _segments = _segmentStore.LoadSegments();
            }

            return _segments.Single(s => s.Id == segmentId);
        }

        private static string SerializeAsJson(PlannedRoute route)
        {
            return JsonConvert.SerializeObject(route, Formatting.Indented, RouteSerializationSettings);
        }
    }
}

