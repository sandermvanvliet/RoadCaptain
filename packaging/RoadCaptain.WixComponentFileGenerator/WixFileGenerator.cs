// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RoadCaptain.WixComponentFileGenerator
{
    public class WixFileGenerator
    {
        private readonly XDocument _doc;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly string _outputPath;
        private readonly XDocument _productDoc;
        private readonly string _productDocOutputPath;

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

            _productDocOutputPath = Path.Combine(Path.GetDirectoryName(outputPath), "Product.wxs");
            
            _productDoc = XDocument.Load(_productDocOutputPath);
            var navigator = _productDoc.Root.CreateNavigator();
            var namespaces = navigator.GetNamespacesInScope(XmlNamespaceScope.All);

            _namespaceManager = new XmlNamespaceManager(new NameTable());

            if (namespaces != null)
            {
                foreach (var ns in namespaces)
                {
                    _namespaceManager.AddNamespace(ns.Key, ns.Value);
                }
            }

            // Need to add the default namespace with a specific prefix (not empty string!)
            // so that we can actually use XPath...
            var defaultNs = _productDoc.Root.GetDefaultNamespace();
            _namespaceManager.AddNamespace("wix", defaultNs.NamespaceName);
        }

        public void Generate(string runnerArtifactsPath, string routeBuilderArtifactsPath)
        {
            if (_doc.Root == null)
            {
                _doc.Add(new XElement(XName.Get("Include")));
            }

            var xPathSelectElement =
                _productDoc.XPathSelectElement("//wix:Directory[@Id='INSTALLFOLDER']", _namespaceManager);

            var runnerFiles = Directory
                .GetFiles(runnerArtifactsPath)
                .Select(Path.GetFileName)
                .ToList();

            var routeBuilderFiles = Directory
                .GetFiles(routeBuilderArtifactsPath)
                .Select(Path.GetFileName)
                .ToList();

            var commonFiles = runnerFiles.Where(file => routeBuilderFiles.Contains(file)).ToList();
            runnerFiles = runnerFiles.Except(commonFiles).ToList();
            routeBuilderFiles = routeBuilderFiles.Except(commonFiles).ToList();

            GenerateFragment(
                runnerArtifactsPath, 
                "INSTALLFOLDER", 
                xPathSelectElement, 
                "",
                "Runner", 
                Directory.GetDirectories(runnerArtifactsPath), 
                runnerFiles);

            GenerateFragment(
                routeBuilderArtifactsPath, 
                "INSTALLFOLDER", 
                xPathSelectElement, 
                "", 
                "RouteBuilder", 
                Directory.GetDirectories(routeBuilderArtifactsPath), 
                routeBuilderFiles);

            GenerateFragment(
                routeBuilderArtifactsPath, 
                "INSTALLFOLDER", 
                xPathSelectElement, 
                "", 
                "Common", 
                Array.Empty<string>(), 
                commonFiles);

            _doc.Save(_outputPath, SaveOptions.OmitDuplicateNamespaces);
            _productDoc.Save(_productDocOutputPath);
        }

        private void GenerateFragment(string path, string componentDirectory, XElement parentDoc, string targetDirPrefix, string prefix, string[] subDirectories, List<string?> files)
        {
            foreach (var subDirectory in subDirectories)
            {
                var subDirectoryName = new DirectoryInfo(subDirectory).Name;
                var dir = parentDoc.XPathSelectElement($"wix:Directory[@Id='{MangleId(subDirectoryName)}']", _namespaceManager);

                if (dir == null)
                {
                    dir = new XElement(XName.Get("Directory", _productDoc.Root.GetDefaultNamespace().NamespaceName));
                    dir.Add(new XAttribute("Id", MangleId(subDirectoryName)));
                    dir.Add(new XAttribute("Name", subDirectoryName));
                    parentDoc.Add(dir);
                }

                var newTargetDirPrefix = targetDirPrefix == "" ? subDirectoryName + "\\" : targetDirPrefix + subDirectoryName + "\\";
                GenerateFragment(subDirectory, subDirectoryName, dir, newTargetDirPrefix, prefix, Directory.GetDirectories(subDirectory), Directory
                    .GetFiles(subDirectory)
                    .Select(Path.GetFileName)
                    .ToList());
            }

            if (files.Any())
            {
                RenderFragment(files,
                    $"{(componentDirectory == "INSTALLFOLDER" ? prefix : componentDirectory)}Components", prefix == "Common" ? "Runner" : prefix,
                    prefix, targetDirPrefix: targetDirPrefix, componentDirectory: componentDirectory);
            }
        }

        private void RenderFragment(List<string> files, string componentId, string targetDir, string prefix = "",
            string targetDirPrefix = "", string componentDirectory = "INSTALLFOLDER")
        {
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

            // Add files missing in the Wix file
            foreach (var file in files)
            {
                var fileWithPrefix = string.IsNullOrEmpty(prefix)
                    ? file
                    : $"{componentId.Replace("Components","")}_{file}";

                fileWithPrefix = MangleId(fileWithPrefix);

                var component = componentGroup.XPathSelectElement($"Component[@Id='{fileWithPrefix}']");

                if (component == null)
                {
                    Console.WriteLine($"File {file} is missing, adding it");

                    var componentFile = new XElement(XName.Get("File"));
                    componentFile.Add(new XAttribute("Id", fileWithPrefix));
                    componentFile.Add(new XAttribute("Name", file));
                    componentFile.Add(new XAttribute("Source",
                        $"$(var.RoadCaptain.{targetDir}_TargetDir){targetDirPrefix}{file}"));
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

            // Remove files that are in the Wix file but no longer published
            foreach (var file in componentGroup.Descendants(XName.Get("File")).ToList())
            {
                var fileName = file.Attribute(XName.Get("Name")).Value;

                if (!files.Contains(fileName))
                {
                    Console.WriteLine($"Removing file {fileName} because it's not part of the release anymore");
                    file.Parent.Remove();
                }
            }
        }

        private string MangleId(string input)
        {
            return input.Replace("-", ".");
        }
    }
}
