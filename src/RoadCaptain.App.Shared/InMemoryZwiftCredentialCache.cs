// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared
{
    public class InMemoryZwiftCredentialCache : IZwiftCredentialCache
    {
        private TokenResponse? _cachedCredentials;

        public async Task StoreAsync(TokenResponse tokenResponse)
        {
            _cachedCredentials = tokenResponse;
        }

        public async Task<TokenResponse?> LoadAsync()
        {
            return _cachedCredentials;
        }
    }
}
