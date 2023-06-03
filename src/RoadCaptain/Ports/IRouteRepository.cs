using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IRouteRepository
    {
        Task<bool> IsAvailableAsync();
        Task<RouteModel[]> SearchAsync(string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null);

        string Name { get; }
    }
}