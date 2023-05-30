using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IRouteRepository
    {
        Task<bool> IsAvailableAsync();
        Task<RouteModel[]> SearchAsync(
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            decimal? minDistance = null,
            decimal? maxDistance = null,
            decimal? minAscent = null,
            decimal? maxAscent = null,
            decimal? minDescent = null,
            decimal? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null);

        string Name { get; }
    }
}