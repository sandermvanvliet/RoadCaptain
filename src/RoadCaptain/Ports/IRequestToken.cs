using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IRequestToken
    {
        Task<OAuthToken> RequestAsync(string username, string password);
    }
}