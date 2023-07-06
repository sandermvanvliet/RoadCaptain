// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    internal class DesignTimeZwiftStub : IZwift
    {
        public Task<Uri?> RetrieveRelayUrl(string accessToken)
        {
            throw new NotImplementedException();
        }

        public Task InitiateRelayAsync(string accessToken, Uri uri, string ipAddress, byte[] connectionSecret)
        {
            throw new NotImplementedException();
        }

        public Task<ZwiftProfile> GetProfileAsync(string accessToken)
        {
            throw new NotImplementedException();
        }

        public Task<OAuthToken> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
