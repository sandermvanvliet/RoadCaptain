// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

        public Models.RouteModel[] Search(string? world, string? creator, string? name, string? zwiftRouteName, decimal? minDistance,
            decimal? maxDistance, decimal? minAscent, decimal? maxAscent, decimal? minDescent, decimal? maxDescent,
            bool? isLoop, string[]? komSegments, string[]? sprintSegments)
        {
            var query = _roadCaptainDataContext.Routes.AsQueryable();

            if (!string.IsNullOrEmpty(world))
            {
                query = query.Where(route => route.World == world);
            }

            if (!string.IsNullOrEmpty(creator))
            {
                query = query.Where(route => route.User != null && route.User.Name  == creator);
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(route => route.Name == name);
            }
            
            if (!string.IsNullOrEmpty(zwiftRouteName))
            {
                query = query.Where(route => route.ZwiftRouteName == zwiftRouteName);
            }

            if (minDistance is > 0)
            {
                query = query.Where(route => route.Distance >= minDistance.Value);
            }

            if (minAscent is > 0)
            {
                query = query.Where(route => route.Ascent >= minAscent.Value);
            }

            if (minDescent is > 0)
            {
                query = query.Where(route => route.Descent >= minDescent.Value);
            }

            if (maxDistance is > 0)
            {
                query = query.Where(route => route.Distance <= maxDistance.Value);
            }

            if (maxAscent is > 0)
            {
                query = query.Where(route => route.Ascent <= maxAscent.Value);
            }


            if (maxDescent is > 0)
            {
                query = query.Where(route => route.Descent <= maxDescent.Value);
            }

            if (isLoop is { })
            {
                query = query.Where(route => route.IsLoop == isLoop);
            }
            
            return query
                .Include(r => r.User)
                .ToList()
                .Select(RouteModelFrom)
                .ToArray();
        }

        public Models.RouteModel? GetById(long id)
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

        public Models.RouteModel Update(long id, UpdateRouteModel updateModel, User currentUser)
        {
            var route = _roadCaptainDataContext
                .Routes
                .Include(route => route.User)
                .SingleOrDefault(r => r.Id == id);

            if (route == null)
            {
                throw new InvalidOperationException("Route not found");
            }

            if (route.User?.Id != currentUser.Id)
            {
                throw new UnauthorizedException("You are not the owner of this route which means you can't change it");
            }

            route.Serialized = updateModel.Serialized;
            route.Distance = updateModel.Distance;
            route.Descent = updateModel.Descent;
            route.Ascent = updateModel.Ascent;
            route.IsLoop = updateModel.IsLoop;
            route.ZwiftRouteName = updateModel.ZwiftRouteName;
            route.Name = updateModel.Name;
            route.Hash = HashUtilities.HashAsHexString(updateModel.Serialized!);

            _roadCaptainDataContext.SaveChanges();

            return RouteModelFrom(route);
        }

        public Models.RouteModel Store(CreateRouteModel createModel, User user)
        {
            var route = RouteStorageModelFrom(createModel, user);

            _roadCaptainDataContext.Routes.Add(route);
            _roadCaptainDataContext.SaveChanges();

            return RouteModelFrom(route);
        }

        public bool Exists(long id)
        {
            return _roadCaptainDataContext
                .Routes
                .AsNoTracking()
                .Any(r => r.Id == id);
        }

        public Dictionary<string, long[]> FindDuplicates()
        {
            var allRoutes = _roadCaptainDataContext
                .Routes
                .Select(r => new
                {
                    r.Id,
                    r.Serialized
                })
                .ToList();

            return allRoutes
                .Select(x => new
                {
                    x.Id,
                    Hash = HashUtilities.HashAsHexString(x.Serialized ?? "null")
                })
                .GroupBy(x => x.Hash,
                    x => x.Id,
                    (hash, ids) => new
                    {
                        Hash = hash,
                        Ids = ids.ToArray()
                    })
                .ToDictionary(x => x.Hash, x => x.Ids);
        }

        public Models.RouteModel[] GetRoutesWithoutHashes()
        {
            return _roadCaptainDataContext
                .Routes
                .Include(route => route.User)
                .AsNoTracking()
                .Where(route => route.Hash == "(not yet calculated)")
                .ToArray()
                .Select(RouteModelFrom)
                .ToArray();
        }

        public Models.RouteModel[] GetAllRoutes()
        {
            return _roadCaptainDataContext
                .Routes
                .Include(route => route.User)
                .AsNoTracking()
                .ToArray()
                .Select(RouteModelFrom)
                .ToArray();
        }

        private static Models.RouteModel RouteModelFrom(Route route)
        {
            return new Models.RouteModel
            {
                Id = route.Id,
                Name = route.Name,
                Ascent = route.Ascent,
                Descent = route.Descent,
                CreatorName = route.User?.Name ?? "(unknown)",
                CreatorZwiftProfileId = route.User?.ZwiftProfileId ?? "(unknown)",
                Distance = route.Distance,
                IsLoop = route.IsLoop,
                ZwiftRouteName = route.ZwiftRouteName,
                Serialized = route.Serialized
            };
        }

        private static Route RouteStorageModelFrom(CreateRouteModel createModel, User user)
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
                Serialized = createModel.Serialized,
                Hash = HashUtilities.HashAsHexString(createModel.Serialized!)
            };
        }
    }
}
