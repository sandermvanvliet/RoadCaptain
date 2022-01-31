using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RoadCaptain.SegmentBuilder
{
    public class Route
    {
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";

        public string Name { get; set; }
        public List<TrackPoint> TrackPoints { get; set; }
        public string Slug { get; set; }

        public static Route FromGpxFile(string filePath)
        {
            var doc = XDocument.Parse(File.ReadAllText(filePath));

            var trkElement = doc.Root.Element(XName.Get("trk", GpxNamespace));

            var trkSeg = trkElement.Elements(XName.Get("trkseg", GpxNamespace));

            var trkpt = trkSeg.Elements(XName.Get("trkpt", GpxNamespace));

            var trackPoints = trkpt
                .Select(trackPoint => new TrackPoint(
                    decimal.Parse(trackPoint.Attribute(XName.Get("lat")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Attribute(XName.Get("lon")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Element(XName.Get("ele", GpxNamespace)).Value,
                        CultureInfo.InvariantCulture)
                ))
                .ToList();

            return new Route
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace)).Value,
                Slug = Path.GetFileNameWithoutExtension(filePath),
                TrackPoints = trackPoints
            };
        }
    }
}