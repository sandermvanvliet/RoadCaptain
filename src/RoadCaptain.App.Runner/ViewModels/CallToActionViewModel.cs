// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class CallToActionViewModel : ViewModelBase
    {
        public CallToActionViewModel(string waitingReason, string instructionText, string backgroundColor = "#1192CC")
        {
            WaitingReason = waitingReason;
            InstructionText = instructionText;
            
            // Because AvaloniaUI marks the Color property of SolidColorBrush as
            // a property that affects rendering, parsing needs to be done on
            // the UI thread...
            SolidColorBrush solidColorBrush;

            if (Dispatcher.UIThread.CheckAccess())
            {
                solidColorBrush = SolidColorBrush.Parse(backgroundColor);
            }
            else
            {
                solidColorBrush = Dispatcher
                    .UIThread
                    .InvokeAsync(() => Task.FromResult(SolidColorBrush.Parse(backgroundColor)))
                    .GetAwaiter()
                    .GetResult();
            }
            
            BackgroundColor = solidColorBrush;
        }

        public string WaitingReason { get; }

        public string InstructionText { get; }
        public SolidColorBrush BackgroundColor { get; }
    }
}
