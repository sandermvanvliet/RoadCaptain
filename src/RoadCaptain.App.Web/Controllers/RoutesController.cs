using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadCaptain.App.Web.Models;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/routes")]
    public class RoutesController : ControllerBase
    {
        private readonly MonitoringEvents _monitoringEvents;

        public RoutesController(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }

        [HttpGet(Name = "GetAllRoutes")]
        public IEnumerable<RouteModel> GetAll(
            [FromQuery] string? world, 
            [FromQuery] string? creator,
            [FromQuery] string? name,
            [FromQuery] string? zwiftRouteName,
            [FromQuery] decimal? minDistance,
            [FromQuery] decimal? maxDistance,
            [FromQuery] decimal? minAscent,
            [FromQuery] decimal? maxAscent,
            [FromQuery] decimal? minDescent,
            [FromQuery] decimal? maxDescent,
            [FromQuery] bool? isLoop,
            [FromQuery] string[]? komSegments,
            [FromQuery] string[]? sprintSegments)
        {
            _monitoringEvents.Information($"Calling GetAllRoutes({world ?? "null"}, {creator ?? "null"})");

            return Array.Empty<RouteModel>();
        }

        [HttpGet("{id}", Name = "GetRouteById")]
        public IActionResult GetRouteById([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteRouteById")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult DeleteRouteById([FromRoute] string id)
        {
            return Unauthorized();
        }

        [HttpPost(Name = "CreateRoute")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult CreateRoute([FromBody] CreateRouteModel createRoute)
        {
            return BadRequest();
        }

        [HttpPut("{id}", Name = "UpdateRouteById")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult UpdateRouteById([FromRoute] string id, [FromBody] UpdateRouteModel updateRoute)
        {
            return BadRequest();
        }
    }
}