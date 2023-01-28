// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class UserInterfaceService : IHostedService
    {
        private readonly IComponentContext _context;
        private readonly MonitoringEvents _monitoringEvents;
        private MainWindow _mainWindow;
        private readonly ISynchronizer _synchronizer;
        private bool _shownBefore;
        private bool _stopping;

        public UserInterfaceService(IComponentContext context, MonitoringEvents monitoringEvents, ISynchronizer synchronizer)
        {
            _context = context;
            _monitoringEvents = monitoringEvents;
            _synchronizer = synchronizer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // We resolve MainWindow here because there is a bunch
            // of setup through Application.whatever() that needs
            // to happen before any WinForm calls are done from
            // within the form itself.
            _mainWindow = _context.Resolve<MainWindow>();

            _mainWindow.Shown += (_, _) =>
            {
                // We're using the Shown event here but that
                // triggers every time the form is shown,
                // therefore we need to debounce and only
                // trigger the synchronization event once.
                if (_shownBefore)
                {
                    return;
                }

                _shownBefore = true;

                //_synchronizer.TriggerSynchronizationEvent();
            };

            // As the form is visible but the console is not we
            // need a callback from closing the form to the host
            Application.ApplicationExit += (_, _) => 
            {
                if (!_stopping)
                {
                    /* stop the host */
                    _synchronizer.RequestApplicationStop();
                }
            };

            _monitoringEvents.ServiceStarted(nameof(UserInterfaceService));

            // Application.Run() blocks but we don't want this service to
            // block. To counter that we'll run it in a task.
            Task.Factory.StartNew(() => Application.Run(_mainWindow), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Prevent re-entry
            if (_stopping)
            {
                return Task.CompletedTask;
            }

            _stopping = true;

            if (_mainWindow.InvokeRequired)
            {
                _mainWindow.Invoke((Action)(() => _mainWindow.Close()));
            }
            else
            {
                _mainWindow?.Close();
            }

            _monitoringEvents.ServiceStopped(nameof(UserInterfaceService));

            return Task.CompletedTask;
        }
    }
}
