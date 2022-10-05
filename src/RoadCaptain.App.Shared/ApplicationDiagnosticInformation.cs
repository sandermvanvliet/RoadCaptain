using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace RoadCaptain.App.Shared
{
    public class ApplicationDiagnosticInformation
    {
        public string TargetFramework { get; private init; }

        public DateTime StartTime { get; private init; }

        public string TargetPlatform { get; private init; }

        public string SupportedPlatform { get; private init; }

        public string Version { get; private init; }

        public string BuildConfiguration { get; private init; }

        public static ApplicationDiagnosticInformation GetFrom(Assembly applicationAssembly)
        {
            var applicationDiagnosticInformation = new ApplicationDiagnosticInformation
            {
                Version = GetVersionFrom(applicationAssembly),
                BuildConfiguration = GetAttributeOf<AssemblyConfigurationAttribute>(applicationAssembly)?.Configuration ?? "(unknown)",
                TargetFramework = GetAttributeOf<TargetFrameworkAttribute>(applicationAssembly)?.FrameworkName ?? "(unknown)",
                TargetPlatform = GetAttributeOf<TargetPlatformAttribute>(applicationAssembly)?.PlatformName ?? "(unknown)",
                SupportedPlatform = GetAttributeOf<SupportedOSPlatformAttribute>(applicationAssembly)?.PlatformName ?? "(unknown)",
                StartTime = DateTime.UtcNow
            };

            return applicationDiagnosticInformation;
        }

        private static string GetVersionFrom(Assembly assembly)
        {
            var informationalVersion = GetAttributeOf<AssemblyInformationalVersionAttribute>(assembly);

            if (informationalVersion != null)
            {
                return informationalVersion.InformationalVersion;
            }

            return GetAttributeOf<AssemblyVersionAttribute>(assembly)?.Version ?? "(unknown)";
        }

        private static TAttribute? GetAttributeOf<TAttribute>(Assembly assembly) where TAttribute : Attribute
        {
            return assembly
                .GetCustomAttributes(typeof(TAttribute))
                .FirstOrDefault() as TAttribute;
        }
    }
}