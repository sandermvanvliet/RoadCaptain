// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class RouteSegmentListViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private SegmentSequenceViewModel? _selectedSegmentSequence;
        private MarkerViewModel? _selectedMarker;

        public RouteSegmentListViewModel(RouteViewModel route, IWindowService windowService)
        {   
            Route = route;
            _windowService = windowService;

            ConfigureLoopCommand = new AsyncRelayCommand(
                    _ => ConfigureLoop(),
                    _ => Route.IsLoop)
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.IsLoop);

            RemoveLastSegmentCommand = new RelayCommand(
                _ => RemoveLastSegment(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route);
        }
        
        public RouteViewModel Route { get; }
        public ICommand ConfigureLoopCommand { get; }
        public ICommand RemoveLastSegmentCommand { get; }

        public SegmentSequenceViewModel? SelectedSegmentSequence
        {
            get => _selectedSegmentSequence;
            set
            {
                if (value == _selectedSegmentSequence) return;
                
                _selectedSegmentSequence = value;
                this.RaisePropertyChanged();
            }
        }

        public MarkerViewModel? SelectedMarker
        {
            get => _selectedMarker;
            set
            {
                if (value == _selectedMarker) return;

                _selectedMarker = value;
                this.RaisePropertyChanged();
            }
        }

        private async Task<CommandResult> ConfigureLoop()
        {
            var shouldCreateLoop = await _windowService.ShowRouteLoopDialog(Route.LoopMode, Route.NumberOfLoops);

            if (!shouldCreateLoop.Success)
            {
                return CommandResult.Aborted();
            }
            
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

        private CommandResult RemoveLastSegment()
        {
            if (!Route.Sequence.Any())
            {
                return CommandResult.Failure("Can't remove segment because the route does not have any segments");
            }

            var lastSegment = Route.RemoveLast();

            SelectedSegmentSequence = null;

            if (lastSegment != null)
            {
                return CommandResult.SuccessWithMessage(lastSegment.SegmentName);
            }

            return CommandResult.Success();
        }
    }
}
