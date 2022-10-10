using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace RoadCaptain.App.Shared
{
    public static class AvaloniaExtensionMethods
    {
        /// <summary>
        /// Capture and restore the location and size of a window
        /// </summary>
        /// <param name="window">The window to track</param>
        /// <param name="windowLocation">The captured state from when the window was launched previously or <c>null</c></param>
        /// <param name="saveAction">Invoked when the window state changes</param>
        /// <remarks>This method restores the position and size at startup (on first activation) and captures the current state when the window is closing.</remarks>
        public static void UseWindowStateTracking(
            this Window window, 
            CapturedWindowLocation? windowLocation, 
            Action<CapturedWindowLocation> saveAction)
        {
            if (windowLocation == null)
            {
                return;
            }

            // ReSharper disable once ConvertToLocalFunction
            EventHandler? handler = null;

            handler = (_, _) =>
            {
                window.Activated -= handler;
                windowLocation.Restore(window);
            };
            
            // This is a two-stage approach. First we register an event handler for when the
            // window is first activated. Then we restore the position and size of the window.
            // This is done because we can only maximize the window upon activation for reasons
            // that I don't understand currently but probably have to do with working out
            // the "true" size of the window by the AvaloniaUI platform implementation.
            window.Activated += handler;

            // Register a handler on Initialized to ensure restore always happens after
            // InitializeComponent() is called and the caller doesn't have to worry about
            // putting this after InitializeComponent() themselves.
            window.Initialized += (_, _) => Restore(windowLocation, window);

            // Register a handler to capture the window state at the time of closing
            window.Closing += (sender, _) =>
            {
                var currentWindow = sender as Window;

                var capturedWindowLocation = currentWindow!.CaptureWindowLocation(windowLocation);
                
                if (capturedWindowLocation != null)
                {
                    saveAction(capturedWindowLocation);
                }
            };
        }

        private static bool CanRestore(this CapturedWindowLocation? windowLocation, Screen screen)
        {
            return windowLocation is { } &&
                   (
                       windowLocation.IsMaximized ||
                       IsPositionWithinScreen(windowLocation.X, windowLocation.Y, screen));
        }

        private static void Restore(this CapturedWindowLocation? windowLocation, Window window)
        {
            if (CanRestore(windowLocation, window.Screens.Primary))
            {
                window.Position = new PixelPoint(
                        windowLocation.X,
                        windowLocation.Y);

                if (windowLocation.Width.HasValue)
                {
                    window.Width = windowLocation.Width.Value;
                }

                if (windowLocation.Height.HasValue)
                {
                    window.Height = windowLocation.Height.Value;
                }

                if (windowLocation.IsMaximized)
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
        }

        private static CapturedWindowLocation? CaptureWindowLocation(this Window window, CapturedWindowLocation? previous)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                // When a window is maximized then the position and size are
                // completely out of whack, so only store that we're maximized
                return new CapturedWindowLocation(
                    previous?.X ?? 0,
                    previous?.Y ?? 0,
                    true,
                    previous?.Width,
                    previous?.Height);
            }

            if (IsPositionWithinScreen(window.Position.X, window.Position.Y, window.Screens.Primary))
            {
                return new CapturedWindowLocation(
                    window.Position.X,
                    window.Position.Y,
                    false, // See above
                    (int)window.Width,
                    (int)window.Height);
            }

            return null;
        }

        private static bool IsPositionWithinScreen(int x, int y, Screen screen)
        {
            return x >= 0 &&
                   x <= screen.Bounds.Width &&
                   y >= 0 &&
                   y <= screen.Bounds.Height;
        }
    }
}