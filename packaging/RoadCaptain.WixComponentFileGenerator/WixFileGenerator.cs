using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoadCaptain.WixComponentFileGenerator
{
    public class WixFileGenerator
    {
        private readonly StreamWriter _writer;

        private const string Template = @"
        <Component Id=""##PREFIX####FILENAME##"" Guid=""##GUID##"">
            <File Id=""##PREFIX####FILENAME##"" Name=""##FILENAME##"" Source=""$(##TARGETDIR##)##TARGETDIRPREFIX####FILENAME##"" ##KEYPATH##/>
        </Component>";

        public WixFileGenerator(string outputPath)
        {
            _writer = new StreamWriter(outputPath);
        }

        public void Generate(string runnerArtifactsPath, string routeBuilderArtifactsPath)
        {
            _writer.WriteLine("<Include>");

            var runnerFiles = Directory.GetFiles(runnerArtifactsPath).Select(Path.GetFileName).ToList();
            var runnerNativeFiles = Directory.GetFiles(Path.Combine(runnerArtifactsPath, "runtimes", "win-x64", "native")).Select(Path.GetFileName).ToList();

            var routeBuilderFiles = Directory.GetFiles(routeBuilderArtifactsPath).Select(Path.GetFileName).ToList();
            var routeBuilderNativeFiles = Directory.GetFiles(Path.Combine(routeBuilderArtifactsPath, "runtimes", "win-x64", "native")).Select(Path.GetFileName).ToList();

            var commonFiles = runnerFiles.Where(file => routeBuilderFiles.Contains(file)).ToList();

            runnerFiles = runnerFiles.Except(commonFiles).ToList();
            routeBuilderFiles = routeBuilderFiles.Except(commonFiles).ToList();
            
            RenderFragment(commonFiles, @"CommonComponents", "Runner");

            RenderFragment(runnerFiles, @"RunnerComponents", "Runner", "Runner");
            RenderFragment(routeBuilderFiles, @"RouteBuilderComponents", "RouteBuilder", "RouteBuilder");
            
            RenderFragment(runnerNativeFiles, @"RunnerNativeComponents", "Runner", "Runner", "runtimes\\win-x64\\native\\");
            RenderFragment(routeBuilderNativeFiles, @"RouteBuilderNativeComponents", "RouteBuilder", "RouteBuilder", "runtimes\\win-x64\\native\\");

            _writer.WriteLine("\n</Include>");
            _writer.Flush();
            _writer.Close();
        }

        private void RenderFragment(List<string> files, string componentId, string targetDir, string prefix = "", string targetDirPrefix = "")
        {
            var componentDirectory = targetDirPrefix.EndsWith("native\\") ? "native" : @"INSTALLFOLDER";

            _writer.Write($@"
<Fragment>
    <ComponentGroup Id=""{componentId}"" Directory=""{componentDirectory}"">");
            foreach (var file in files)
            {
                var generated = RenderTemplate(file, $"var.RoadCaptain.{targetDir}_TargetDir", prefix == "" ? "" : $"{prefix}_", targetDirPrefix);

                _writer.Write(generated);
            }

            _writer.Write(@"
    </ComponentGroup>
</Fragment>");
        }

        private static string RenderTemplate(string file, string targetDir, string prefix, string targetDirPrefix)
        {
            return Template
                .Replace("##FILENAME##", file)
                .Replace("##GUID##", Guid.NewGuid().ToString("D"))
                .Replace("##TARGETDIR##", targetDir)
                .Replace("##TARGETDIRPREFIX##", targetDirPrefix)
                .Replace("##PREFIX##", prefix)
                .Replace("##KEYPATH##", file.EndsWith(".exe") ? "KeyPath=\"yes\" " : "");
        }
    }
}