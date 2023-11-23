using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface ISecurityTokenProvider
    {
        Task<string?> GetSecurityTokenForPurposeAsync(TokenPurpose purpose);
    }
}