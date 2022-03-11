using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.Runner.HostedServices;
using RoadCaptain.Runner.ViewModels;
using Point = System.Drawing.Point;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for InGameNavigationWindow.xaml
    /// </summary>
    public partial class InGameNavigationWindow : Window
    {
        private readonly ISynchronizer _synchronizer;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly CancellationTokenSource _tokenSource = new();
        private InGameNavigationWindowViewModel _viewModel;


        public InGameNavigationWindow(
            ISynchronizer synchronizer,
            IGameStateReceiver gameStateReceiver, 
            MonitoringEvents monitoringEvents)
        {
            _synchronizer = synchronizer;
            _monitoringEvents = monitoringEvents;
            
            // Start the receiver whenever we trigger the synchronization event
            _synchronizer.RegisterStart(() => Task.Factory.StartNew(() => gameStateReceiver.Start(_tokenSource.Token)));
            
            InitializeComponent();

            gameStateReceiver.Register(
                null,
                null,
                GameStateReceived);
        }

        private void Window_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void InGameNavigationWindow_OnInitialized(object? sender, EventArgs e)
        {
            if (AppSettings.Default.InGameWindowLocation != Point.Empty)
            {
                Left = AppSettings.Default.InGameWindowLocation.X;
                Top = AppSettings.Default.InGameWindowLocation.Y;
            }
        }

        private void InGameNavigationWindow_OnLocationChanged(object sender, EventArgs e)
        {
            AppSettings.Default.InGameWindowLocation = new Point((int)Left, (int)Top);
            AppSettings.Default.Save();
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
            _viewModel = DataContext as InGameNavigationWindowViewModel;

            // Not to worry about multiple OnActivated events, TriggerSynchronizationEvent is idempotent
            _synchronizer.TriggerSynchronizationEvent();
        }

        private void GameStateReceived(GameState gameState)
        {
            try
            {
                _viewModel.UpdateGameState(gameState);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to update game state");
            }
        }
    }
}