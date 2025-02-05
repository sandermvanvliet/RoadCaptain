// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.ViewModels;

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
            set => SetProperty(ref _selectedSegmentSequence, value);
        }

        public MarkerViewModel? SelectedMarker
        {
            get => _selectedMarker;
            set => SetProperty(ref _selectedMarker, value);
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
                OnPropertyChanged(nameof(Route));
            }
            else
            {
                // Clear the loop properties
                foreach (var seq in Route.Sequence.Where(s => s.IsLoop))
                {
                    seq.Type = SegmentSequenceType.Regular;
                }
                OnPropertyChanged(nameof(Route));
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
