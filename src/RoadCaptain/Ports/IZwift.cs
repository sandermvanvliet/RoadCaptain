// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IZwift
    {
        Task<Uri> RetrieveRelayUrl(string accessToken);
        Task InitiateRelayAsync(string accessToken, Uri uri, string ipAddress, byte[] connectionSecret);
        Task<ZwiftProfile> GetProfileAsync(string accessToken);
        Task<OAuthToken> RefreshTokenAsync(string refreshToken);
    }
}
