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
                .SubscribeTo(this, () => Route.Sequence)
                .OnSuccess(_ =>
                {
                    // TODO: fix me
                    //Model.StatusBarInfo("Removed segment");
                })
                .OnSuccessWithMessage(_ =>
                {
                    // TODO: fix me
                    //Model.StatusBarInfo("Removed segment {0}", _.Message);
                })
                .OnFailure(_ =>
                {
                    // TODO: fix me
                    //Model.StatusBarWarning(_.Message);
                });
        }
        public RouteViewModel Route { get; }
        public ICommand ConfigureLoopCommand { get; }
        public ICommand RemoveLastSegmentCommand { get; }


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

        private CommandResult RemoveLastSegment()
        {
            if (!Route.Sequence.Any())
            {
                return CommandResult.Failure("Can't remove segment because the route does not have any segments");
            }

            var lastSegment = Route.RemoveLast();

            // TODO: fix me
            //SelectedSegment = null;

            if (lastSegment != null)
            {
                return CommandResult.SuccessWithMessage(lastSegment.SegmentName);
            }

            return CommandResult.Success();
        }
    }
}