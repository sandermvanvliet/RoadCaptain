using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared
{
    public interface IZwiftCredentialCache
    {
        Task StoreAsync(TokenResponse tokenResponse);
        Task<TokenResponse?> LoadAsync();
    }
}