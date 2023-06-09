using System.ComponentModel;
using System.Diagnostics;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using WindowsCredentialManager;

namespace RoadCaptain.App.Windows
{
    public class CredentialCache : IZwiftCredentialCache
    {
        private const string ZwiftAccessTokenTargetName = "RoadCaptain.Zwift.AccessToken";
        private const string ZwiftRefreshTokenTargetName = "RoadCaptain.Zwift.RefreshToken";
        
        public Task StoreAsync(TokenResponse tokenResponse)
        {
            try
            {
                StoreSecretByName(tokenResponse.AccessToken ?? "", ZwiftAccessTokenTargetName);
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }

            try
            {
                StoreSecretByName(tokenResponse.RefreshToken ?? "", ZwiftRefreshTokenTargetName);
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }

            return Task.CompletedTask;
        }

        public Task<TokenResponse?> LoadAsync()
        {
            var accessToken = LoadSecretByName(ZwiftAccessTokenTargetName);
            var refreshToken = LoadSecretByName(ZwiftRefreshTokenTargetName);

            if (string.IsNullOrEmpty(accessToken))
            {
                return Task.FromResult<TokenResponse?>(null);
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Task.FromResult<TokenResponse?>(null);
            }

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Task.FromResult<TokenResponse?>(tokenResponse);
        }

        private static void StoreSecretByName(
            string secret, string targetName)
        {
            var credential = new RawStringCredentials(targetName)
            {
                UserName = "",
                Password = secret,
                Persistence = CredentialPersistence.LocalMachine
            };
            credential.Save();
        }

        private string? LoadSecretByName(string targetName)
        {
            var credential = new RawStringCredentials(targetName);

            try
            {
                credential.Load();

                if (credential.Password == null)
                {
                    return null;
                }

                return credential.Password;
            }
            catch (Win32Exception e) when(e.NativeErrorCode == 1168)
            {
                return null;
            }
        }
    }
}