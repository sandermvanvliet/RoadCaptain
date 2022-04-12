using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Views
{
    public class InGameWindowTests
    {
        //[StaFact]
        public void GivenRouteInLastSegment_SecondSegmentRowIsNotVisible()
        {
            var segments = new List<Segment>
            {
                new(new List<TrackPoint>()) { Id = "seg-1"},
                new(new List<TrackPoint>()) { Id = "seg-2"},
                new(new List<TrackPoint>()) { Id = "seg-3"},
            };
            var route = new PlannedRoute()
            {
                Name = "TestRoute",
                World = "TestWorld",
                ZwiftRouteName = "Mountain route"
            };
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", NextSegmentId = null, Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.EnteredSegment("seg-1");

            var model = new InGameWindowModel(segments)
            {
                Route = route
            };

            var viewModel = new InGameNavigationWindowViewModel(model, segments);
            var monitoringEvents = new NopMonitoringEvents();
            var window =
                new InGameNavigationWindow(new InMemoryGameStateDispatcher(monitoringEvents), monitoringEvents)
                    {
                        ShowActivated = true,
                        DataContext = viewModel
                    };

            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-2");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), segments[1], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-3");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), segments[2], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);
            
            var windowContent = (window.Content as Grid);
            //TryScreenshotToClipboardAsync(windowContent).GetAwaiter().GetResult();
            var secondRow = (windowContent.FindName("SecondRow") as Grid);

            secondRow
                .Visibility
                .Should()
                .Be(Visibility.Collapsed);
        }

        //[StaFact]
        public void GivenRouteInLastSegment_PlaceholderIsVisible()
        {
            var segment = new Segment(new List<TrackPoint>
            {
                new TrackPoint(1, 2, 3),
                new TrackPoint(1, 2.1, 5)
            }) { Id = "seg-3"};
            segment.CalculateDistances();
            var segments = new List<Segment>
            {
                new(new List<TrackPoint>()) { Id = "seg-1"},
                new(new List<TrackPoint>()) { Id = "seg-2"},
                segment,
            };
            var route = new PlannedRoute()
            {
                Name = "TestRoute",
                World = "TestWorld",
                ZwiftRouteName = "Mountain route"
            };
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", NextSegmentId = null, Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.EnteredSegment("seg-1");

            var model = new InGameWindowModel(segments)
            {
                Route = route
            };

            var viewModel = new InGameNavigationWindowViewModel(model, segments);
            var monitoringEvents = new NopMonitoringEvents();
            var window =
                new InGameNavigationWindow(new InMemoryGameStateDispatcher(monitoringEvents), monitoringEvents)
                    {
                        ShowActivated = true,
                        DataContext = viewModel
                    };
            viewModel.Model.TotalDescent = 123;
            viewModel.Model.TotalAscent = 78;
            viewModel.Model.TotalDistance = 25;
            viewModel.Model.ElapsedDescent = 33;
            viewModel.Model.ElapsedAscent = 12;
            viewModel.Model.ElapsedDistance = 25;

            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-2");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), segments[1], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-3");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3) { DistanceOnSegment = 12}, segments[2], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);
            
            var windowContent = (window.Content as Grid);
            TryScreenshotToClipboardAsync(windowContent).GetAwaiter().GetResult();
            var secondRow = (windowContent.FindName("Placeholder") as Grid);

            secondRow
                .Visibility
                .Should()
                .Be(Visibility.Visible);
        }

        //[StaFact]
        public void GivenRouteHasCompleted_FinishFlagIsVisible()
        {
            var segment = new Segment(new List<TrackPoint>
            {
                new TrackPoint(1, 2, 3),
                new TrackPoint(1, 2.1, 5)
            }) { Id = "seg-3"};
            segment.CalculateDistances();
            var segments = new List<Segment>
            {
                new(new List<TrackPoint>()) { Id = "seg-1"},
                new(new List<TrackPoint>()) { Id = "seg-2"},
                segment,
            };
            var route = new PlannedRoute()
            {
                Name = "TestRoute",
                World = "TestWorld",
                ZwiftRouteName = "Mountain route"
            };
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", NextSegmentId = null, Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.EnteredSegment("seg-1");

            var model = new InGameWindowModel(segments)
            {
                Route = route
            };

            var viewModel = new InGameNavigationWindowViewModel(model, segments);
            var monitoringEvents = new NopMonitoringEvents();
            var window =
                new InGameNavigationWindow(new InMemoryGameStateDispatcher(monitoringEvents), monitoringEvents)
                    {
                        ShowActivated = true,
                        DataContext = viewModel
                    };
            viewModel.Model.TotalDescent = 123;
            viewModel.Model.TotalAscent = 78;
            viewModel.Model.TotalDistance = 25;
            viewModel.Model.ElapsedDescent = 33;
            viewModel.Model.ElapsedAscent = 12;
            viewModel.Model.ElapsedDistance = 25;

            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-2");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), segments[1], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-3");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3) { DistanceOnSegment = 12}, segments[2], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            viewModel.UpdateGameState(new CompletedRouteState(1, 2, new TrackPoint(1, 2, 3) { DistanceOnSegment = 12}, route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);
            
            var windowContent = (window.Content as Grid);
            TryScreenshotToClipboardAsync(windowContent).GetAwaiter().GetResult();
            var secondRow = (windowContent.FindName("FinishFlag") as StackPanel);

            secondRow
                .Visibility
                .Should()
                .Be(Visibility.Visible);
        }

        //[StaFact]
        public void GivenRouteInLastSegmentButNotYetCompleted_FinishFlagIsNotVisible()
        {
            var segment = new Segment(new List<TrackPoint>
            {
                new TrackPoint(1, 2, 3),
                new TrackPoint(1, 2.1, 5)
            }) { Id = "seg-3"};
            segment.CalculateDistances();
            var segments = new List<Segment>
            {
                new(new List<TrackPoint>()) { Id = "seg-1"},
                new(new List<TrackPoint>()) { Id = "seg-2"},
                segment,
            };
            var route = new PlannedRoute()
            {
                Name = "TestRoute",
                World = "TestWorld",
                ZwiftRouteName = "Mountain route"
            };
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3", Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", NextSegmentId = null, Direction = SegmentDirection.AtoB, TurnToNextSegment = TurnDirection.GoStraight });
            route.EnteredSegment("seg-1");

            var model = new InGameWindowModel(segments)
            {
                Route = route
            };

            var viewModel = new InGameNavigationWindowViewModel(model, segments);
            var monitoringEvents = new NopMonitoringEvents();
            var window =
                new InGameNavigationWindow(new InMemoryGameStateDispatcher(monitoringEvents), monitoringEvents)
                    {
                        ShowActivated = true,
                        DataContext = viewModel
                    };
            viewModel.Model.TotalDescent = 123;
            viewModel.Model.TotalAscent = 78;
            viewModel.Model.TotalDistance = 25;
            viewModel.Model.ElapsedDescent = 33;
            viewModel.Model.ElapsedAscent = 12;
            viewModel.Model.ElapsedDistance = 25;

            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-2");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), segments[1], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);

            route.EnteredSegment("seg-3");
            viewModel.UpdateGameState(new OnRouteState(1, 2, new TrackPoint(1, 2, 3) { DistanceOnSegment = 12}, segments[2], route));
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);
            
            var windowContent = (window.Content as Grid);
            TryScreenshotToClipboardAsync(windowContent).GetAwaiter().GetResult();
            var secondRow = (windowContent.FindName("FinishFlag") as StackPanel);

            secondRow
                .Visibility
                .Should()
                .Be(Visibility.Collapsed);
        }

        public async Task<bool> TryScreenshotToClipboardAsync(FrameworkElement frameworkElement)
        {
            frameworkElement.ClipToBounds = true; // Can remove if everything still works when the screen is maximised.

            Rect relativeBounds = VisualTreeHelper.GetDescendantBounds(frameworkElement);
            double areaWidth = frameworkElement.RenderSize.Width; // Cannot use relativeBounds.Width as this may be incorrect if a window is maximised.
            double areaHeight = frameworkElement.RenderSize.Height; // Cannot use relativeBounds.Height for same reason.
            double XLeft = relativeBounds.X;
            double XRight = XLeft + areaWidth;
            double YTop = relativeBounds.Y;
            double YBottom = YTop + areaHeight;
            var bitmap = new RenderTargetBitmap((int)Math.Round(XRight, MidpointRounding.AwayFromZero),
                                                (int)Math.Round(YBottom, MidpointRounding.AwayFromZero),
                                                96, 96, PixelFormats.Default);

            // Render framework element to a bitmap. This works better than any screen-pixel-scraping methods which will pick up unwanted
            // artifacts such as the taskbar or another window covering the current window.
            var dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                var vb = new VisualBrush(frameworkElement);
                ctx.DrawRectangle(vb, null, new Rect(new Point(XLeft, YTop), new Point(XRight, YBottom)));
            }
            bitmap.Render(dv);
            
            return await TryCopyBitmapToClipboard(bitmap);
        }

        private static async Task<bool> TryCopyBitmapToClipboard(BitmapSource bmpCopied)
        {
            var tries = 3;
            while (tries-- > 0)
            {
                try
                {
                    // This must be executed on the calling dispatcher.
                    Clipboard.SetImage(bmpCopied);
                    return true;
                }
                catch (COMException)
                {
                    // Windows clipboard is optimistic concurrency. On fail (as in use by another process), retry.
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
            return false;
        }
    }
}