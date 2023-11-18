﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class RouteSegmentListViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;

        public RouteSegmentListViewModel(RouteViewModel route, IWindowService windowService)
        {
            Route = route;
            _windowService = windowService;

            ConfigureLoopCommand = new AsyncRelayCommand(
                    _ => ConfigureLoop(),
                    _ => Route.IsLoop)
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.IsLoop);
        }
        public RouteViewModel Route { get; }
        public ICommand ConfigureLoopCommand { get; }


        private async Task<CommandResult> ConfigureLoop()
        {
            var shouldCreateLoop = await _windowService.ShowRouteLoopDialog(Route.LoopMode, Route.NumberOfLoops);

            if (shouldCreateLoop.Mode is LoopMode.Infinite or LoopMode.Constrained)
            {
                Route.LoopMode = shouldCreateLoop.Mode;
                Route.NumberOfLoops = shouldCreateLoop.NumberOfLoops;
                this.RaisePropertyChanged(nameof(Route));
            }
            else
            {
                // Clear the loop properties
                foreach (var seq in Route.Sequence.Where(s => s.IsLoop))
                {
                    seq.Type = SegmentSequenceType.Regular;
                }
                this.RaisePropertyChanged(nameof(Route));
            }

            return CommandResult.Success();
        }
    }
}