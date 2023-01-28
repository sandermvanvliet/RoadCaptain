// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class NoZwiftCredentialCache : IZwiftCredentialCache
    {
        public async Task StoreAsync(TokenResponse tokenResponse)
        {
        }
        
        public async Task<TokenResponse?> LoadAsync()
        {
            return null;
        }
    }
}
