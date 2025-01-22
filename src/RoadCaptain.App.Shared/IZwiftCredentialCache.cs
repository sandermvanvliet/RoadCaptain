// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared
{
    public interface IZwiftCredentialCache
    {
        Task StoreAsync(TokenResponse tokenResponse);
        Task<TokenResponse?> LoadAsync();
    }
}
