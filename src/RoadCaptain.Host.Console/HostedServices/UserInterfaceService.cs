using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using Microsoft.Extensions.Hosting;
using RoadCaptain.Monitor;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class UserInterfaceService : IHostedService
    {
        private readonly IComponentContext _context;

        public UserInterfaceService(IComponentContext context)
        {
            _context = context;
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
            var mainWindow = _context.Resolve<MainWindow>();
            
            // As the form is visible but the console is not we
            // need a callback from closing the form to the host
            mainWindow.FormClosed += (sender, args) =>
            {
                /* stop the host */
                
            };

            Application.Run(mainWindow);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Exit();

            return Task.CompletedTask;
        }
    }
}