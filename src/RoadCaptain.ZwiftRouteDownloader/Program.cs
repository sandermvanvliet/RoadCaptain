using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RoadCaptain.ZwiftRouteDownloader
{
    internal class Program
    {
        private readonly HttpClient _client;
        private readonly string _outputDirectory;
        private readonly string _world;
        private readonly string _zwiftDataRepositoryPath;
        private static readonly Regex PropertyNameRegex = new("[ \t]([a-zA-Z0-9]*):", RegexOptions.Compiled);
        private static readonly Regex TrailingCommaRegex = new(",$\n[ \t]*[\\]|}]", RegexOptions.Compiled | RegexOptions.Multiline);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Zwift route downloader");
            
            if (args.Length != 2)
            {
                var appName = Assembly.GetExecutingAssembly().GetName().Name;
                await Console.Error.WriteLineAsync($"Usage: {appName} <world> <zwift data git repository path>");
                Environment.ExitCode = 1;
                return;
            }

            var program = new Program("watopia", args[1]);
            
            await program.DownloadRoutes();

            await program.DownloadSegments();
        }

        private Program(string world, string zwiftDataRepositoryPath)
        {
            _client = new HttpClient();
            _world = world;
            _zwiftDataRepositoryPath = zwiftDataRepositoryPath;
            _outputDirectory = $@"c:\git\temp\zwift\zwift-{world}-gpx";
        }

        private async Task DownloadRoutes()
        {
            Console.WriteLine("Downloading routes");

            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            var routesPath = Path.Combine(_zwiftDataRepositoryPath, "data", "routes.mjs");
            var routes = JsonConvert.DeserializeObject<Route[]>(await ReadZwiftDataEcmaScriptModuleAsJsonAsync(routesPath));

            var routesForWorld = routes
                .Where(route => route.World == _world)
                .ToList();

            foreach (var route in routesForWorld)
            {
                if (!File.Exists(GpxFileNameOf(route)))
                {
                    Console.Write($"Downloading GPX of {route.Name}...");
                    try
                    {
                        if (route.StravaSegmentId.HasValue)
                        {
                            await DownloadRoute(route);
                            Console.WriteLine(" [OK]");
                        }
                        else
                        {
                            Console.WriteLine(" [SKIP] Route doesn't have a Strava segment id");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($" [FAILED] {e.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($" [SKIP] Route has already been downloaded");
                }
            }
        }

        private static async Task<string> ReadZwiftDataEcmaScriptModuleAsJsonAsync(string routesPath)
        {
            var input = await File.ReadAllTextAsync(routesPath);

            var step1 = input[input.IndexOf('[')..];
            var step2 = step1[..^3];
            var step3 = PropertyNameRegex.Replace(step2, match => $"\"{match.Groups[1]}\":");
            var step4 = TrailingCommaRegex.Replace(step3, _ => ",");
            
            return step4;
        }

        private async Task DownloadSegments()
        {
            Console.WriteLine("Downloading segments");
            
            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            var segmentsPath = Path.Combine(_zwiftDataRepositoryPath, "data", "segments.mjs");
            var segments = JsonConvert.DeserializeObject<Segment[]>(await ReadZwiftDataEcmaScriptModuleAsJsonAsync(segmentsPath));

            var routesOnWorld = segments
                .Where(route => route.World == _world)
                .ToList();

            foreach (var segment in routesOnWorld)
            {
                if (!File.Exists(GpxFileNameOf(segment)))
                {
                    Console.Write($"Downloading GPX of {segment.Name}...");
                    try
                    {
                        await DownloadSegment(segment);
                        Console.WriteLine(" [OK]");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($" [FAILED] {e.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($" [SKIP] Segment has already been downloaded");
                }
            }
        }

        private async Task DownloadRoute(Route route)
        {
            var url = $"https://www.strava.com/stream/segments/{route.StravaSegmentId}?streams[]=latlng&streams[]=altitude";

            var result = await _client.GetStringAsync(url);

            var stravaSegment = JsonConvert.DeserializeObject<StravaSegment>(result);

            var gpx = BuildGpx(route, stravaSegment);

            await File.WriteAllTextAsync(Path.Combine(_outputDirectory, GpxFileNameOf(route)), gpx);
        }

        private async Task DownloadSegment(Segment segment)
        {
            var url = $"https://www.strava.com/stream/segments/{segment.StravaSegmentId}?streams[]=latlng&streams[]=altitude";

            var result = await _client.GetStringAsync(url);

            var stravaSegment = JsonConvert.DeserializeObject<StravaSegment>(result);

            var gpx = BuildGpx(segment, stravaSegment);

            await File.WriteAllTextAsync(Path.Combine(_outputDirectory, GpxFileNameOf(segment)), gpx);
        }

        private static string GpxFileNameOf(Route route)
        {
            return $"{route.World}-{route.Slug}.gpx";
        }

        private static string GpxFileNameOf(Segment segment)
        {
            return $"{segment.World}-{segment.Slug}.gpx";
        }

        private string BuildGpx(Route segmentData, StravaSegment stravaSegment)
        {
            var name = $"{segmentData.Name} ({segmentData.World})";
            var link = $"{segmentData.StravaSegmentUrl}";


            var trkptList = stravaSegment
                .LatLng
                .Select((x, index) => $"<trkpt lat=\"{x[0].ToString(CultureInfo.InvariantCulture)}\" lon=\"{x[1].ToString(CultureInfo.InvariantCulture)}\"><ele>{stravaSegment.Altitude[index].ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                .ToList();

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<gpx creator=\"RoadCaptain:ZwiftRouteDownloader\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                "<trk>" +
                $"<name>{name}</name>"+
                $"<desc>0km</desc>" +
                $"<link href=\"{link}\">" + 
                $"<type>{string.Join(",", segmentData.Sports)}</type>" +
                "</link>" +
                $"<trkseg>{string.Join(Environment.NewLine, trkptList)}</trkseg>" +
                "</trk>" +
                "</gpx>";
        }

        private string BuildGpx(Segment segmentData, StravaSegment stravaSegment)
        {
            var name = $"{segmentData.Name} ({segmentData.World})";
            var distance = $"{segmentData.Distance}km";
            var link = $"{segmentData.StravaSegmentUrl}";
            
            var trkptList = stravaSegment
                .LatLng
                .Select((x, index) => $"<trkpt lat=\"{x[0].ToString(CultureInfo.InvariantCulture)}\" lon=\"{x[1].ToString(CultureInfo.InvariantCulture)}\"><ele>{stravaSegment.Altitude[index].ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                .ToList();

            var type = segmentData.Type;
            if ("segment".Equals(type, StringComparison.InvariantCultureIgnoreCase))
            {
                type = "stravaSegment";
            }

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<gpx creator=\"RoadCaptain:ZwiftRouteDownloader\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>{name}</name>"+
                   $"<desc>{distance}</desc>" +
                   $"<link href=\"{link}\">" + 
                   $"<type>{type}</type>" +
                   "</link>" +
                   $"<trkseg>{string.Join(Environment.NewLine, trkptList)}</trkseg>" +
                   "</trk>" +
                   "</gpx>";
        }
    }
}
