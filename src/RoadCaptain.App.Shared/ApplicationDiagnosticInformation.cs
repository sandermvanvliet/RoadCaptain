// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RoadCaptain.App.Shared
{
    public class ApplicationDiagnosticInformation
    {
        public string? Name { get; private init; }
        public string? TargetFramework { get; private init; }

        public DateTime StartTime { get; private init; }

        public string? TargetPlatform { get; private init; }

        public string? SupportedPlatform { get; private init; }

        public string? Version { get; private init; }

        public string? BuildConfiguration { get; private init; }

        public string? RuntimeIdentifier { get; set; }

        public string? RuntimeFrameworkDescription { get; set; }
        
        public string? RuntimeOSDescription { get; set; }

        public static ApplicationDiagnosticInformation GetFrom(Assembly assembly)
        {
            var applicationDiagnosticInformation = new ApplicationDiagnosticInformation
            {
                Name = assembly.GetName().Name ?? "(unknown)",
                Version = GetVersionFrom(assembly),
                BuildConfiguration = GetAttributeOf<AssemblyConfigurationAttribute>(assembly)?.Configuration ?? "(unknown)",
                TargetFramework = GetAttributeOf<TargetFrameworkAttribute>(assembly)?.FrameworkName ?? "(unknown)",
                TargetPlatform = GetAttributeOf<TargetPlatformAttribute>(assembly)?.PlatformName ?? "(unknown)",
                SupportedPlatform = GetAttributeOf<SupportedOSPlatformAttribute>(assembly)?.PlatformName ?? "(unknown)",
                StartTime = DateTime.UtcNow,
                RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
                RuntimeFrameworkDescription = RuntimeInformation.FrameworkDescription,
                RuntimeOSDescription = RuntimeInformation.OSDescription
            };

            return applicationDiagnosticInformation;
        }

        private static string? GetVersionFrom(Assembly assembly)
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
