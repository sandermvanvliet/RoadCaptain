// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Shared
{
    public class AppSecurityTokenProvider : ISecurityTokenProvider
    {
        private readonly IWindowService _windowService;
        private readonly IZwiftCredentialCache _credentialCache;
        private readonly IZwift _zwift;

        public AppSecurityTokenProvider(IWindowService windowService, IZwiftCredentialCache credentialCache, IZwift zwift)
        {
            _windowService = windowService;
            _credentialCache = credentialCache;
            _zwift = zwift;
        }
        
        public async Task<string?> GetSecurityTokenForPurposeAsync(
            TokenPurpose purpose,
            TokenPromptBehaviour promptBehaviour)
        {
            if (purpose == TokenPurpose.Unknown)
            {
                throw new ArgumentException("Won't provide a token for an unknown purpose");
            }

            if (purpose is TokenPurpose.RouteRepositoryAccess or TokenPurpose.ZwiftGameAccess)
            {
                var cachedCredentials = await AuthenticateToZwiftAsync(promptBehaviour);

                return cachedCredentials?.AccessToken;
            }
            
            throw new ArgumentException("Won't provide a token for an unsupported purpose");
        }
        
        private async Task<TokenResponse?> AuthenticateToZwiftAsync(TokenPromptBehaviour tokenPromptBehaviour)
        {
            var tokenResponse = await _credentialCache.LoadAsync();

            if (tokenResponse != null)
            {
                if (!string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    var accessToken = new JsonWebToken(tokenResponse.AccessToken);

                    if (accessToken.ValidTo < DateTime.UtcNow.AddHours(1))
                    {
                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            var refreshToken = new JsonWebToken(tokenResponse.RefreshToken);

                            if (refreshToken.ValidTo < DateTime.UtcNow.AddHours(1))
                            {
                                tokenResponse = null;
                            }
                            else
                            {
                                try
                                {
                                    var refreshedTokens = await _zwift.RefreshTokenAsync(tokenResponse.RefreshToken);

                                    tokenResponse = new TokenResponse
                                    {
                                        AccessToken = refreshedTokens.AccessToken,
                                        RefreshToken = refreshedTokens.RefreshToken,
                                        ExpiresIn = (long)refreshedTokens.ExpiresOn.Subtract(DateTime.UtcNow).TotalSeconds,
                                        UserProfile = tokenResponse.UserProfile
                                    };

                                    await _credentialCache.StoreAsync(tokenResponse);
                                }
                                catch
                                {
                                    tokenResponse = null;
                                }
                            }
                        }
                        else
                        {
                            tokenResponse = null;
                        }
                    }
                }
                else
                {
                    tokenResponse = null;
                }
            }

            if (tokenResponse != null || tokenPromptBehaviour == TokenPromptBehaviour.DoNotPrompt)
            {
                return tokenResponse;
            }

            var currentWindow = _windowService.GetCurrentWindow();
            if (currentWindow == null)
            {
                throw new InvalidOperationException(
                    "Unable to determine what the current window and I can't parent a dialog to an unknown window");
            }

            tokenResponse = await _windowService.ShowLogInDialog(currentWindow);

            if (tokenResponse != null &&
                !string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                // Keep this in memory so that when the app navigates
                // from the in-game window to the main window the user
                // remains logged in.
                await _credentialCache.StoreAsync(tokenResponse);
            }

            return tokenResponse;
        }
    }
}
