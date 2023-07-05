// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.Runner
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents) 
            : base(componentContext, monitoringEvents)
        {
        }

        public void ToggleElevationPlot(PlannedRoute? plannedRoute, bool? show)
        {
            var elevationPlot = CurrentWindow!.OwnedWindows.OfType<ElevationPlotWindow>().SingleOrDefault();
            var userPreferences = Resolve<IUserPreferences>();

            if (elevationPlot != null)
            {
                elevationPlot.Close();
                userPreferences.ShowElevationPlotInGame = false;
                userPreferences.Save();
            }
            else if(plannedRoute != null)
            {
                elevationPlot = Resolve<ElevationPlotWindow>();
                
                var viewModel = Resolve<ElevationPlotWindowViewModel>();
                viewModel.UpdateRoute(plannedRoute);

                elevationPlot.DataContext = viewModel;
                
                userPreferences.ShowElevationPlotInGame = true;
                userPreferences.Save();

                elevationPlot.Show(CurrentWindow);
            }
        }

        public async Task<RouteModel?> ShowSelectRouteDialog()
        {
            var selectRouteWindow = Resolve<SelectRouteWindow>();

            selectRouteWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(selectRouteWindow) ?? false)
            {
                return selectRouteWindow.SelectedRoute;
            }

            return null;
        }

        public void ShowMainWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                CurrentWindow.Activate();
                return;
            }

            var mainWindow = Resolve<MainWindow>();
            
            ApplicationLifetime!.MainWindow = mainWindow;
            
            SwapWindows(mainWindow);
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            if (CurrentWindow is InGameNavigationWindow)
            {
                //CurrentWindow.Activate();
                return;
            }

            var inGameWindow = Resolve<InGameNavigationWindow>();

            inGameWindow.DataContext = viewModel;

            ApplicationLifetime!.MainWindow = inGameWindow;

            SwapWindows(inGameWindow);
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = Resolve<ZwiftLoginWindowBase>();

            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(zwiftLoginWindow) ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }
    }
}
