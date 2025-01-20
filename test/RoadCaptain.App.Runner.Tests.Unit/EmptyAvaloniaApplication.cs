using System;
using Avalonia;

namespace RoadCaptain.App.Runner.Tests.Unit
{
    public class EmptyAvaloniaApplication : Application
    {
        private static AppBuilder? _appBuilder;
        private static readonly object SyncRoot = new();

        public static void EnsureInitializedForTesting()
        {
            if (_appBuilder != null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_appBuilder == null)
                {
                    try
                    {
                        _appBuilder = AppBuilder
                            .Configure<EmptyAvaloniaApplication>()
                            .UsePlatformDetect()
                            .SetupWithoutStarting();
                    }
                    catch (InvalidOperationException)
                    {
                        // Nop
                    }
                }
            }
        }
    }
}