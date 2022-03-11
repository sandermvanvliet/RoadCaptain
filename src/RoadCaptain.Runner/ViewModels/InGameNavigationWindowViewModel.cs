using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class InGameNavigationWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel)
        {
            Model = inGameWindowModel;
        }
        
        public InGameWindowModel Model { get; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateGameState(GameState gameState)
        {
            if (gameState is OnRouteState)
            {

            }
        }
    }
}