using System;
using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IZwift
    {
        Task<Uri> RetrieveRelayUrl(string accessToken);
        Task InitiateRelayAsync(string accessToken, Uri uri, string ipAddress, byte[]? connectionSecret);
        Task<ZwiftProfile> GetProfileAsync(string accessToken);
        Task<OAuthToken> RefreshTokenAsync(string refreshToken);
    }
}