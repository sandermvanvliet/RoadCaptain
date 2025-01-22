// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared
{
    public class InMemoryZwiftCredentialCache : IZwiftCredentialCache
    {
        private TokenResponse? _cachedCredentials;

        public Task StoreAsync(TokenResponse tokenResponse)
        {
            _cachedCredentials = tokenResponse;
            return Task.CompletedTask;
        }

        public Task<TokenResponse?> LoadAsync()
        {
            return Task.FromResult(_cachedCredentials);
        }
    }
}
