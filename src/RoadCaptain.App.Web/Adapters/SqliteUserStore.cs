﻿using System.Security.Claims;
using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web.Adapters
{
    internal class SqliteUserStore : IUserStore
    {
        private readonly RoadCaptainDataContext _roadCaptainDataContext;

        public SqliteUserStore(RoadCaptainDataContext roadCaptainDataContext)
        {
            _roadCaptainDataContext = roadCaptainDataContext;
        }

        public User? GetOrCreate(ClaimsPrincipal principal)
        {
            var subjectClaim = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (subjectClaim == null)
            {
                return null;
            }

            var user = _roadCaptainDataContext.Users.SingleOrDefault(u => u.ZwiftSubject == subjectClaim.Value);

            if (user == null)
            {
                user = new User
                {
                    ZwiftProfileId = "",
                    Name = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name).Value,
                    ZwiftSubject = subjectClaim.Value
                };
                _roadCaptainDataContext.Users.Add(user);
                _roadCaptainDataContext.SaveChanges();
            }

            return user;
        }
    }
}