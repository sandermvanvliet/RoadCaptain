// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RouteStoreToDisk : IRouteStore
    {
        internal static readonly JsonSerializerSettings RouteSerializationSettings = new()
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
            _currentVersion = GetType().Assembly.GetName().Version ?? new Version(0, 0, 0, 1);
        }

        public PlannedRoute LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Couldn't find '{path}'");
            }

            var serialized = File.ReadAllText(path);

            return DeserializeAndUpgrade(serialized);
        }

        internal PlannedRoute DeserializeAndUpgrade(string serialized)
        {
            var parsed = JObject.Parse(serialized);
            
            PlannedRoute? plannedRoute;

            if (parsed.ContainsKey("version"))
            {
                var schemaVersion = parsed["version"]?.Value<string>() ?? "0";

                if (schemaVersion == PersistedRouteVersion1.Version)
                {
                    var deserialized = JsonConvert.DeserializeObject<PersistedRouteVersion1>(
                        serialized,
                        RouteSerializationSettings);

                    if (deserialized == null || deserialized.Route == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize to a valid route object");
                    }

                    if (deserialized.Route.WorldId == null)
                    {
                        throw new InvalidOperationException("Expected route to have a WorldId but it didn't have one");
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
                    
                    // Routes created before 0.6.5.0 did not have a segment sequence type
                    SetSegmentSequenceType(plannedRoute);
                }
                else if (schemaVersion == PersistedRouteVersion2.Version)
                {
                    var deserialized = JsonConvert.DeserializeObject<PersistedRouteVersion2>(
                        serialized,
                        RouteSerializationSettings);

                    if (deserialized == null || deserialized.Route == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize to a valid route object");
                    }

                    if (string.IsNullOrEmpty(deserialized.RoadCaptainVersion))
                    {
                        throw new InvalidOperationException(
                            "Route does not specify the RoadCaptain version it was created with, I can't determine what to do now");
                    }

                    if (Version.Parse(deserialized.RoadCaptainVersion) > _currentVersion)
                    {
                        throw new InvalidOperationException(
                            "Route was created with a newer version of RoadCaptain and that won't work");
                    }

                    if (deserialized.Route.WorldId == null)
                    {
                        throw new InvalidOperationException("Expected route to have a WorldId but it didn't have one");
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

                    if (plannedRoute.World == null)
                    {
                        throw new InvalidOperationException(
                            "Route doesn't specify for which world it is, I don't know what to do with that");
                    }

                    // If the route is a loop then we need to ensure that the
                    // turn to the next segment on the last segment is set 
                    // correctly as all version 2 routes don't have that set.
                    if (plannedRoute.IsLoop)
                    {
                        var firstSegmentSequence = plannedRoute.RouteSegmentSequence.FirstOrDefault(seg => seg.Type == SegmentSequenceType.LoopStart);
                        // Handle any possible situation where we haven't done the Loop -> LoopStart conversion
                        firstSegmentSequence ??= plannedRoute.RouteSegmentSequence.First(seg => seg.Type == SegmentSequenceType.Loop);

                        var lastSegmentSequence = plannedRoute.RouteSegmentSequence.Last(seg => seg.Type == SegmentSequenceType.LoopEnd);

                        var segments = _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport);
                        var lastSegment = segments.Single(s => s.Id == lastSegmentSequence.SegmentId);

                        var turns = lastSegmentSequence.Direction == SegmentDirection.AtoB
                            ? lastSegment.NextSegmentsNodeB
                            : lastSegment.NextSegmentsNodeA;

                        var turnToNext = turns.Single(t => t.SegmentId == firstSegmentSequence.SegmentId);

                        lastSegmentSequence.TurnToNextSegment = turnToNext.Direction;

                        // For routes created pre-0.7.0.0 we only support infinite loop mode
                        // as there was no way to indicate how many times a loop should be 
                        // followed.
                        plannedRoute.LoopMode = LoopMode.Infinite;
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

                    if (deserialized == null || deserialized.Route == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize to a valid route object");
                    }

                    if (string.IsNullOrEmpty(deserialized.RoadCaptainVersion))
                    {
                        throw new InvalidOperationException(
                            "Route does not specify the RoadCaptain version it was created with, I can't determine what to do now");
                    }

                    var routeVersion = Version.Parse(deserialized.RoadCaptainVersion);

                    if (routeVersion > _currentVersion)
                    {
                        throw new InvalidOperationException(
                            "Route was created with a newer version of RoadCaptain and that won't work");
                    }

                    if (routeVersion < new Version(0, 6, 8, 0))
                    {
                        throw new InvalidOperationException(
                            "The route file has version 3 but was created with a version of RoadCaptain that does not support version 3, did you manually change the file?");
                    }

                    if (deserialized.Route.WorldId == null)
                    {
                        throw new InvalidOperationException("Expected route to have a WorldId but it didn't have one");
                    }

                    if (routeVersion < new Version(0, 7, 0, 0) && deserialized.Route.WorldId.StartsWith("makuri"))
                    {
                        throw new InvalidOperationException(
                            "Segments for Makuri Islands changed too much since version 0.7.0.0 to automatically upgrade to this version of RoadCaptain. Please rebuild your route in Route Builder");
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
                    if (routeVersion < new Version(0, 6, 6, 0))
                    {
                        SetSegmentSequenceType(plannedRoute);
                    }

                    if (plannedRoute.World == null)
                    {
                        throw new InvalidOperationException(
                            "Route doesn't specify for which world it is, I don't know what to do with that");
                    }

                    // If the route is a loop then we need to ensure that the
                    // turn to the next segment on the last segment is set 
                    // correctly as all version 2 routes don't have that set.
                    if (plannedRoute.IsLoop)
                    {
                        var firstSegmentSequence = plannedRoute.RouteSegmentSequence.FirstOrDefault(seg => seg.Type == SegmentSequenceType.LoopStart);
                        // Handle any possible situation where we haven't done the Loop -> LoopStart conversion
                        firstSegmentSequence ??= plannedRoute.RouteSegmentSequence.First(seg => seg.Type == SegmentSequenceType.Loop);
                        
                        var lastSegmentSequence = plannedRoute.RouteSegmentSequence.Last(seg => seg.Type == SegmentSequenceType.LoopEnd);

                        var segments = _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport);
                        var lastSegment = segments.Single(s => s.Id == lastSegmentSequence.SegmentId);

                        var turns = lastSegmentSequence.Direction == SegmentDirection.AtoB
                            ? lastSegment.NextSegmentsNodeB
                            : lastSegment.NextSegmentsNodeA;

                        var turnToNext = turns.Single(t => t.SegmentId == firstSegmentSequence.SegmentId);

                        lastSegmentSequence.TurnToNextSegment = turnToNext.Direction;

                        if (routeVersion < new Version(0, 7, 0, 0))
                        {
                            plannedRoute.LoopMode = LoopMode.Infinite;
                        }
                    }

                    if (routeVersion < new Version(0, 7, 0, 7))
                    {
                        // With version 0.7.0.9 we've added new segments to Watopia
                        // and split a few which we need to correct for.

                        void SplitSequence(List<SegmentSequence> newSequence, string segmentId, SegmentSequence sequence, int index)
                        {
                            if (sequence.Direction == SegmentDirection.AtoB)
                            {
                                newSequence.Add(
                                    new SegmentSequence(
                                        $"{segmentId}-before",
                                        sequence.Type, 
                                        sequence.Direction,
                                        index+1));
                                newSequence[^1].TurnToNextSegment = TurnDirection.GoStraight;
                                newSequence[^1].NextSegmentId = $"{segmentId}-after";
                                
                                newSequence.Add(
                                    new SegmentSequence(
                                        $"{segmentId}-after",
                                        sequence.Type, 
                                        sequence.Direction,
                                        index+2));
                                newSequence[^1].TurnToNextSegment = sequence.TurnToNextSegment;
                                newSequence[^1].NextSegmentId = sequence.NextSegmentId;
                            }
                            else
                            {
                                newSequence.Add(
                                    new SegmentSequence(
                                        $"{segmentId}-after",
                                        sequence.Type, 
                                        sequence.Direction,
                                        index+1));
                                newSequence[^1].TurnToNextSegment = TurnDirection.GoStraight;
                                newSequence[^1].NextSegmentId = $"{segmentId}-before";
                                
                                newSequence.Add(
                                    new SegmentSequence(
                                        $"{segmentId}-before",
                                        sequence.Type, 
                                        sequence.Direction,
                                        index+2));
                                newSequence[^1].TurnToNextSegment = sequence.TurnToNextSegment;
                                newSequence[^1].NextSegmentId = sequence.NextSegmentId;
                            }
                        }
                        
                        var newSequence = new List<SegmentSequence>();
                        var index = 0;
                        var tainted = false;
                        
                        foreach (var sequence in plannedRoute.RouteSegmentSequence)
                        {
                            if (sequence.SegmentId == "watopia-bambino-fondo-003-after-after-before")
                            {
                                // 1: watopia-bambino-fondo-003-after-after-before is split
                                SplitSequence(newSequence, "watopia-bambino-fondo-003-after-after-before", sequence, index);
                                index += 2;
                                tainted = true;
                            }
                            else if (sequence.SegmentId == "watopia-tempus-fugit-001")
                            {
                                // 2: watopia-tempus-fugit-001 is split
                                SplitSequence(newSequence, "watopia-tempus-fugit-001", sequence, index);
                                index += 2;
                                tainted = true;
                            }
                            else if (sequence.SegmentId == "watopia-bambino-fondo-004-after-before")
                            {
                                // 3: watopia-bambino-fondo-004-after-before is split
                                SplitSequence(newSequence, "watopia-bambino-fondo-004-after-before", sequence, index);
                                index += 2;
                                tainted = true;
                            }
                            else
                            {
                                sequence.Index = index++;
                                newSequence.Add(sequence);
                            }
                        }

                        if (tainted)
                        {
                            plannedRoute.RouteSegmentSequence.Clear();
                            plannedRoute.RouteSegmentSequence.AddRange(newSequence);
                        }
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

                if (deserializeObject == null)
                {
                    throw new InvalidOperationException("Failed to deserialize to a valid route object");
                }

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

            return plannedRoute!;
        }

        /// <summary>
        /// Set the segment sequence type on the route
        /// </summary>
        /// <remarks>This method ensures that the type is set for routes that were created with earlier versions of RoadCaptain</remarks>
        /// <param name="route"></param>
        private static void SetSegmentSequenceType(PlannedRoute route)
        {
            if (RouteIsALoop(route))
            {
                foreach (var sequence in route.RouteSegmentSequence)
                {
                    sequence.Type = SegmentSequenceType.Loop;
                }

                route.RouteSegmentSequence.First().Type = SegmentSequenceType.LoopStart;
                route.RouteSegmentSequence.Last().Type = SegmentSequenceType.LoopEnd;
            }
            else
            {
                foreach (var sequence in route.RouteSegmentSequence)
                {
                    sequence.Type = SegmentSequenceType.Regular;
                }
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

        public async Task<Uri> StoreAsync(PlannedRoute route, string path)
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

            await File.WriteAllTextAsync(path, serialized);

            return new Uri(path);
        }

        internal static string SerializeAsJson(PlannedRoute route, Formatting formatting = Formatting.Indented)
        {
            var versionedRoute = new PersistedRouteVersion3
            {
                RoadCaptainVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4) ?? throw new Exception("Unable to determine RoadCaptain version"),
                Route = route
            };

            return JsonConvert.SerializeObject(versionedRoute, formatting, RouteSerializationSettings);
        }

        private string SerializeAsGpx(PlannedRoute route)
        {
            if (route.World == null)
            {
                throw new ArgumentException("Can't serialize this route because it does not have a world defined");
            }

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