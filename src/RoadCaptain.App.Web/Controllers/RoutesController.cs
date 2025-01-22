// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadCaptain.App.Web.Models;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/routes")]
    public class RoutesController : ControllerBase
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IRouteStore _routeStore;
        private readonly IUserStore _userStore;

        public RoutesController(MonitoringEvents monitoringEvents, IRouteStore routeStore, IUserStore userStore)
        {
            _monitoringEvents = monitoringEvents;
            _routeStore = routeStore;
            _userStore = userStore;
        }

        [HttpGet(Name = "GetAllRoutes")]
        public IEnumerable<Models.RouteModel> GetAll(
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

            return _routeStore.Search(world, creator, name, zwiftRouteName, minDistance, maxDistance, minAscent, maxAscent, minDescent, maxDescent, isLoop, komSegments, sprintSegments);
        }

        [HttpGet("{id:long}", Name = "GetRouteById")]
        public IActionResult GetRouteById([FromRoute] long id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            var route = _routeStore.GetById(id);

            if (route == null)
            {
                return NotFound();
            }

            return Ok(route);
        }

        [HttpDelete("{id:long}", Name = "DeleteRouteById")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult DeleteRouteById([FromRoute] long id)
        {
            _routeStore.Delete(id);

            return Ok();
        }

        [HttpPost(Name = "CreateRoute")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult CreateRoute([FromBody] CreateRouteModel createRoute)
        {
            var user = _userStore.GetOrCreate(HttpContext.User);
            if (user == null)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext,
                    (int)HttpStatusCode.BadRequest, "Invalid user"));
            }

            try
            {
                return Ok(_routeStore.Store(createRoute, user));
            }
            catch (DuplicateRouteException e)
            {
                _monitoringEvents.Error(e, "Duplicate route exists");
                
                return Conflict(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    (int)HttpStatusCode.Conflict,
                    "Duplicate route exists"));
            }
        }

        [HttpPut("{id:long}", Name = "UpdateRouteById")]
        [Authorize(Policy = "ZwiftUserPolicy")]
        public IActionResult UpdateRouteById([FromRoute] long id, [FromBody] UpdateRouteModel updateRoute)
        {
            if (!_routeStore.Exists(id))
            {
                return NotFound();
            }

            var currentUser = _userStore.GetOrCreate(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                return Ok(_routeStore.Update(id, updateRoute, currentUser));
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
        }
    }
}
