// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RouteStoreToDisk : IRouteStore
    {
        private static readonly JsonSerializerSettings RouteSerializationSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };

        private readonly ISegmentStore _segmentStore;
        private readonly IWorldStore _worldStore;
        private readonly Version _currentVersion;

        public RouteStoreToDisk(ISegmentStore segmentStore, IWorldStore worldStore)
        {
            _segmentStore = segmentStore;
            _worldStore = worldStore;
            _currentVersion = GetType().Assembly.GetName().Version;
        }

        public PlannedRoute LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            var serialized = File.ReadAllText(path);
            var parsed = JObject.Parse(serialized);

            PlannedRoute plannedRoute;

            if (parsed.ContainsKey("version"))
            {
                var schemaVersion = parsed["version"]?.Value<string>() ?? "0";

                if (schemaVersion == PersistedRouteVersion1.Version)
                {
                    var deserialized = JsonConvert.DeserializeObject<PersistedRouteVersion1>(
                        serialized,
                        RouteSerializationSettings);

                    deserialized.Route.World = _worldStore.LoadWorldById(deserialized.Route.WorldId);
                    
                    // For routes that were created before sport was known
                    // set it to Cycling because we only supported bike rides.
                    if (deserialized.Route.Sport == SportType.Unknown)
                    {
                        deserialized.Route.Sport = SportType.Cycling;
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    plannedRoute = deserialized.Route;
                    
                    // Routes created before 0.6.5.0 did not have a segment sequence type
                    SetSegmentSequenceType(plannedRoute);
                }
                else if (schemaVersion == PersistedRouteVersion2.Version)
                {
                    var deserialized = JsonConvert.DeserializeObject<PersistedRouteVersion2>(
                        serialized,
                        RouteSerializationSettings);

                    if (Version.Parse(deserialized.RoadCaptainVersion) > _currentVersion)
                    {
                        throw new InvalidOperationException(
                            "Route was created with a newer version of RoadCaptain and that won't work");
                    }

                    deserialized.Route.World = _worldStore.LoadWorldById(deserialized.Route.WorldId);
                    
                    // For routes that were created before sport was known
                    // set it to Cycling because we only supported bike rides.
                    if (deserialized.Route.Sport == SportType.Unknown)
                    {
                        deserialized.Route.Sport = SportType.Cycling;
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    plannedRoute = deserialized.Route;

                    // Routes created before 0.6.6.0 did not have a segment sequence type
                    if (Version.Parse(deserialized.RoadCaptainVersion) < new Version(0, 6, 6, 0))
                    {
                        SetSegmentSequenceType(plannedRoute);
                    }

                    // If the route contains a turn from:
                    // watopia-bambino-fondo-004-after-after
                    // to:
                    // watopia-bambino-fondo-004-after-before
                    // we need to fix the direction because it was wrong in the
                    // turn file for Watopia
                    var matchingSequences = plannedRoute
                        .RouteSegmentSequence
                        .Where(seq => seq.SegmentId == "watopia-bambino-fondo-004-after-after" &&
                                      seq.NextSegmentId == "watopia-bambino-fondo-004-after-before")
                        .ToList();

                    foreach (var sequuence in matchingSequences)
                    {
                        sequuence.TurnToNextSegment = TurnDirection.Right;
                    }
                }
                else if (schemaVersion == PersistedRouteVersion3.Version)
                {
                    var deserialized = JsonConvert.DeserializeObject<PersistedRouteVersion2>(
                        serialized,
                        RouteSerializationSettings);

                    if (Version.Parse(deserialized.RoadCaptainVersion) > _currentVersion)
                    {
                        throw new InvalidOperationException(
                            "Route was created with a newer version of RoadCaptain and that won't work");
                    }

                    if (Version.Parse(deserialized.RoadCaptainVersion) < new Version(0, 6, 8, 0))
                    {
                        throw new InvalidOperationException(
                            "The route file has version 3 but was created with a version of RoadCaptain that does not support version 3, did you manually change the file?");
                    }

                    deserialized.Route.World = _worldStore.LoadWorldById(deserialized.Route.WorldId);
                    
                    // For routes that were created before sport was known
                    // set it to Cycling because we only supported bike rides.
                    if (deserialized.Route.Sport == SportType.Unknown)
                    {
                        deserialized.Route.Sport = SportType.Cycling;
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    plannedRoute = deserialized.Route;

                    // Routes created before 0.6.6.0 did not have a segment sequence type
                    if (Version.Parse(deserialized.RoadCaptainVersion) < new Version(0, 6, 6, 0))
                    {
                        SetSegmentSequenceType(plannedRoute);
                    }
                }
                else
                {
                    throw new Exception("Don't understand route version") { Data = { { "Version", schemaVersion } } };
                }
            }
            else
            {
                var deserializeObject = JsonConvert.DeserializeObject<PersistedRouteVersion0>(
                    serialized,
                    RouteSerializationSettings);

                // ReSharper disable once PossibleNullReferenceException
                plannedRoute = deserializeObject.AsRoute(_worldStore.LoadWorldById("watopia"));

                SetSegmentSequenceType(plannedRoute);
            }

            if (plannedRoute != null)
            {
                // Handle the spawn point segment split
                if (plannedRoute.StartingSegmentId == "watopia-bambino-fondo-001-after-after-after-after-after")
                {
                    if ("mountain route".Equals(plannedRoute.ZwiftRouteName,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        plannedRoute.RouteSegmentSequence[0].SegmentId = "watopia-bambino-fondo-001-after-after-after-after-after-after";
                    }
                    else
                    {
                        plannedRoute.RouteSegmentSequence[0].SegmentId = "watopia-bambino-fondo-001-after-after-after-after-after-before";
                    }
                }
            }

            return plannedRoute;
        }

        /// <summary>
        /// Set the segment sequence type on the route
        /// </summary>
        /// <remarks>This method ensures that the type is set for routes that were created with earlier versions of RoadCaptain</remarks>
        /// <param name="route"></param>
        private static void SetSegmentSequenceType(PlannedRoute route)
        {
            var type = SegmentSequenceType.Regular;

            if (RouteIsALoop(route))
            {
                type = SegmentSequenceType.Loop;
            }

            foreach (var sequence in route.RouteSegmentSequence)
            {
                sequence.Type = type;
            }
        }

        private static bool RouteIsALoop(PlannedRoute route)
        {
            // We can't use IsLoop here because that relies on
            // the SegmentSequenceType of each segment sequence.
            // Therefore we need to check that the last sequence
            // has a turn direction None and a NextSegmentId 
            // that is the first segment of the route

            var firstSequence = route.RouteSegmentSequence[0];
            var lastSequence = route.RouteSegmentSequence[^1];

            if (lastSequence.TurnToNextSegment == TurnDirection.None &&
                lastSequence.NextSegmentId == firstSequence.SegmentId)
            {
                return true;
            }

            return false;
        }

        public void Store(PlannedRoute route, string path)
        {
            // Ensure that segment sequences have at least
            // the type regular if it's not been set.
            foreach (var sequence in route.RouteSegmentSequence)
            {
                if (sequence.Type == SegmentSequenceType.Unknown)
                {
                    sequence.Type = SegmentSequenceType.Regular;
                }
            }

            var serialized = path.EndsWith(".gpx", StringComparison.InvariantCultureIgnoreCase)
                ? SerializeAsGpx(route)
                : SerializeAsJson(route);

            File.WriteAllText(path, serialized);
        }

        private static string SerializeAsJson(PlannedRoute route)
        {
            var versionedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4) ?? throw new Exception("Unable to determine RoadCaptain version"),
                Route = route
            };

            return JsonConvert.SerializeObject(versionedRoute, Formatting.Indented, RouteSerializationSettings);
        }

        private string SerializeAsGpx(PlannedRoute route)
        {
            var segments = _segmentStore.LoadSegments(route.World, route.Sport);

            var trackSegments = string.Join(
                Environment.NewLine,
                route.RouteSegmentSequence.Select(seg =>
                    "<trkseg>" + SegmentAsTrackPointList(seg, segments) + "</trkseg>"));

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4) ?? "0.0.0.0";
            var routeBuilderVersion = $"RoadCaptain:RouteBuilder {version}";

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   $"<gpx creator=\"{routeBuilderVersion}\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>{route.Name}</name>" +
                   $"<desc>RoadCaptain route starting at {route.ZwiftRouteName}</desc>" +
                   "<type>RoadCaptain route</type>" +
                   trackSegments +
                   "</trk>" +
                   "</gpx>";
        }

        private string SegmentAsTrackPointList(SegmentSequence segment, List<Segment> world)
        {
            var actualSegment = world.Single(s => s.Id == segment.SegmentId);

            return string.Join(
                Environment.NewLine,
                actualSegment
                    .Points
                    .Select(x =>
                        $"<trkpt lat=\"{x.Latitude.ToString(CultureInfo.InvariantCulture)}\" lon=\"{x.Longitude.ToString(CultureInfo.InvariantCulture)}\"><ele>{x.Altitude.ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                    .ToList());
        }
    }
}