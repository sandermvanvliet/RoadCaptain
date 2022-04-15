using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RoadCaptain.WixComponentFileGenerator
{
    public class WixFileGenerator
    {
        private readonly XDocument _doc;
        private readonly string _outputPath;

        public WixFileGenerator(string outputPath)
        {
            _outputPath = outputPath;

            if (File.Exists(outputPath))
            {
                _doc = XDocument.Load(outputPath);
            }
            else
            {
                _doc = new XDocument();
            }
        }

        public void Generate(string runnerArtifactsPath, string routeBuilderArtifactsPath)
        {
            var runnerFiles = Directory.GetFiles(runnerArtifactsPath).Select(Path.GetFileName).ToList();
            var runnerNativeFiles = Directory.GetFiles(Path.Combine(runnerArtifactsPath, "runtimes", "win-x64", "native")).Select(Path.GetFileName).ToList();

            var routeBuilderFiles = Directory.GetFiles(routeBuilderArtifactsPath).Select(Path.GetFileName).ToList();
            var routeBuilderNativeFiles = Directory.GetFiles(Path.Combine(routeBuilderArtifactsPath, "runtimes", "win-x64", "native")).Select(Path.GetFileName).ToList();

            var commonFiles = runnerFiles.Where(file => routeBuilderFiles.Contains(file)).ToList();

            runnerFiles = runnerFiles.Except(commonFiles).ToList();
            routeBuilderFiles = routeBuilderFiles.Except(commonFiles).ToList();
            
            if (_doc.Root == null)
            {
                _doc.Add(new XElement(XName.Get("Include")));
            }

            RenderFragment(commonFiles, @"CommonComponents", "Runner");

            RenderFragment(runnerFiles, @"RunnerComponents", "Runner", "Runner");
            RenderFragment(routeBuilderFiles, @"RouteBuilderComponents", "RouteBuilder", "RouteBuilder");
            
            RenderFragment(runnerNativeFiles, @"RunnerNativeComponents", "Runner", "Runner", "runtimes\\win-x64\\native\\");
            RenderFragment(routeBuilderNativeFiles, @"RouteBuilderNativeComponents", "RouteBuilder", "RouteBuilder", "runtimes\\win-x64\\native\\");
            
            _doc.Save(_outputPath, SaveOptions.OmitDuplicateNamespaces);
        }

        private void RenderFragment(List<string> files, string componentId, string targetDir, string prefix = "", string targetDirPrefix = "")
        {
            var componentDirectory = targetDirPrefix.EndsWith("native\\") ? "native" : @"INSTALLFOLDER";

            var componentGroup = _doc.XPathSelectElement($"/Include/Fragment/ComponentGroup[@Id='{componentId}']");
            
            if (componentGroup == null)
            {
                Console.WriteLine($"Component {componentId} is missing, adding it");
                var fragment = new XElement(XName.Get("Fragment"));
                componentGroup = new XElement(XName.Get("ComponentGroup"));
                componentGroup.Add(new XAttribute(XName.Get("Id"), componentId));
                componentGroup.Add(new XAttribute(XName.Get("Directory"), componentDirectory));
                fragment.Add(componentGroup);
                _doc.Root.Add(fragment);
            }

            foreach (var file in files)
            {
                var fileWithPrefix = string.IsNullOrEmpty(prefix)
                    ? file
                    : $"{prefix}_{file}";

                var component = componentGroup.XPathSelectElement($"Component[@Id='{fileWithPrefix}']");

                if (component == null)
                {
                    Console.WriteLine($"File {file} is missing, adding it");

                    var componentFile = new XElement(XName.Get("File"));
                    componentFile.Add(new XAttribute("Id", fileWithPrefix));
                    componentFile.Add(new XAttribute("Name", file));
                    componentFile.Add(new XAttribute("Source", $"$(var.RoadCaptain.{targetDir}_TargetDir){targetDirPrefix}{file}"));
                    if (file.EndsWith(".exe"))
                    {
                        componentFile.Add(new XAttribute(XName.Get("KeyPath"), "yes"));
                    }

                    component = new XElement(XName.Get("Component"));
                    component.Add(new XAttribute(XName.Get("Id"), fileWithPrefix));
                    component.Add(new XAttribute(XName.Get("Guid"), Guid.NewGuid().ToString("D")));
                    component.Add(componentFile);
                    
                    componentGroup.Add(component);
                }
            }
        }
    }
}