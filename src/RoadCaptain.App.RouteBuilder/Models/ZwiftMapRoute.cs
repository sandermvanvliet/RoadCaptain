// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RoadCaptain.App.RouteBuilder.Models
{
    public class ZwiftMapRoute
    {
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";

        public string? Name { get; private init; }
        public List<TrackPoint> TrackPoints { get; private init; } = new();
        public string? Slug { get; private init; }
        public string[]? Sports { get; private init; }
        public string? WorldName { get; set; }
        public string? Link { get; private init; }

        public static ZwiftMapRoute FromGpxFile(string filePath)
        {
            var doc = XDocument.Parse(File.ReadAllText(filePath));

            if (doc.Root == null)
            {
                throw new Exception("GPX file is invalid as it doesn't have an XML root node");
            }

            var trkElement = doc.Root.Element(XName.Get("trk", GpxNamespace));

            if (trkElement == null)
            {
                throw new Exception("GPX file is invalid as I couldn't find trk element");
            }

            var linkElement = trkElement.Element(XName.Get("link", GpxNamespace));
            var typeElement = linkElement?.Element(XName.Get("type", GpxNamespace));
            var trkSeg = trkElement.Elements(XName.Get("trkseg", GpxNamespace));

            var trkptElements = trkSeg.Elements(XName.Get("trkpt", GpxNamespace));
            
#pragma warning disable CS8602
            var trackPoints = trkptElements
                .Select(trackPoint => new TrackPoint(
                    double.Parse(trackPoint.Attribute(XName.Get("lat")).Value, CultureInfo.InvariantCulture),
                    double.Parse(trackPoint.Attribute(XName.Get("lon")).Value, CultureInfo.InvariantCulture),
                    double.Parse(trackPoint.Element(XName.Get("ele", GpxNamespace)).Value,
                        CultureInfo.InvariantCulture)
                ))
                .ToList();
#pragma warning restore CS8602

            var link = linkElement?.Attribute(XName.Get("href"))?.Value;

            var sports = typeElement?.Value.Split(',') ?? new[] { "running", "cycling" };
            
            var worldName = "watopia";
            
            if (!string.IsNullOrEmpty(link))
            {
                worldName = link.Split("/")[3];
            }
            
            return new ZwiftMapRoute
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace))?.Value,
                Slug = Path.GetFileNameWithoutExtension(filePath),
                TrackPoints = trackPoints,
                Sports = sports,
                Link = link,
                WorldName = worldName
            };
        }
    }
}

