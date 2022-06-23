using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class NoZwiftCredentialCache : IZwiftCredentialCache
    {
        public async Task StoreAsync(TokenResponse tokenResponse)
        {
        }
        
        public async Task<TokenResponse?> LoadAsync()
        {
            return null;
        }
    }
}