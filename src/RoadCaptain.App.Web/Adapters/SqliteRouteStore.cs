using Microsoft.EntityFrameworkCore;
using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Models;
using RoadCaptain.App.Web.Ports;
using Route = RoadCaptain.App.Web.Adapters.EntityFramework.Route;

namespace RoadCaptain.App.Web.Adapters
{
    internal class SqliteRouteStore : IRouteStore
    {
        private readonly RoadCaptainDataContext _roadCaptainDataContext;

        public SqliteRouteStore(RoadCaptainDataContext roadCaptainDataContext)
        {
            _roadCaptainDataContext = roadCaptainDataContext;
        }

        public RouteModel[] Search(string? world, string? creator, string? name, string? zwiftRouteName, decimal? minDistance,
            decimal? maxDistance, decimal? minAscent, decimal? maxAscent, decimal? minDescent, decimal? maxDescent,
            bool? isLoop, string[]? komSegments, string[]? sprintSegments)
        {
            return _roadCaptainDataContext
                .Routes
                .Include(r => r.User)
                .ToList()
                .Select(RouteModelFrom)
                .ToArray();
        }

        public RouteModel? GetById(long id)
        {
            var route = _roadCaptainDataContext
                .Routes
                .Include(r => r.User)
                .SingleOrDefault(r => r.Id == id);

            return route == null
                ? null
                : RouteModelFrom(route);
        }

        public void Delete(long id)
        {
            var route = _roadCaptainDataContext
                .Routes
                .SingleOrDefault(r => r.Id == id);

            if (route != null)
            {
                _roadCaptainDataContext.Routes.Remove(route);
                _roadCaptainDataContext.SaveChanges();
            }
        }

        public RouteModel? Update(long id, UpdateRouteModel updateModel)
        {
            var route = _roadCaptainDataContext
                .Routes
                .SingleOrDefault(r => r.Id == id);

            if (route == null)
            {
                return null;
            }

            _roadCaptainDataContext.SaveChanges();

            return RouteModelFrom(route);
        }

        public RouteModel Store(CreateRouteModel createModel, User user)
        {
            var route = RouteStorageModelFrom(createModel, user);

            _roadCaptainDataContext.Routes.Add(route);
            _roadCaptainDataContext.SaveChanges();

            return RouteModelFrom(route);
        }

        private static RouteModel RouteModelFrom(Route route)
        {
            return new RouteModel
            {
                Id = route.Id,
                Name = route.Name,
                Ascent = route.Ascent,
                Descent = route.Descent,
                CreatorName = route.User.Name,
                CreatorZwiftProfileId = route.User.ZwiftProfileId,
                Distance = route.Distance,
                IsLoop = route.IsLoop,
                ZwiftRouteName = route.ZwiftRouteName,
                Serialized = route.Serialized
            };
        }

        private Route RouteStorageModelFrom(CreateRouteModel createModel, User user)
        {
            return new Route
            {
                Name = createModel.Name,
                Ascent = createModel.Ascent,
                Descent = createModel.Descent,
                User = user,
                Distance = createModel.Distance,
                IsLoop = createModel.IsLoop,
                ZwiftRouteName = createModel.ZwiftRouteName,
                Serialized = createModel.Serialized
            };
        }
    }
}