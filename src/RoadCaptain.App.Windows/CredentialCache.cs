using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using WindowsCredentialManager;

namespace RoadCaptain.App.Windows
{
    public class CredentialCache : IZwiftCredentialCache
    {
        private const string ZwiftAccessTokenTargetName = "RoadCaptain.Zwift.AccessToken";
        private const string ZwiftRefreshTokenTargetName = "RoadCaptain.Zwift.RefreshToken";
        private const string ZwiftUserProfileTargetName = "RoadCaptain.Zwift.UserProfile";

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None
        };

        public Task StoreAsync(TokenResponse tokenResponse)
        {
            StoreSecretByName(tokenResponse.AccessToken ?? "", ZwiftAccessTokenTargetName);

            StoreSecretByName(tokenResponse.RefreshToken ?? "", ZwiftRefreshTokenTargetName);
            
            if (tokenResponse.UserProfile != null)
            {
                StoreSecretByName(SerializeUserProfile(tokenResponse.UserProfile), ZwiftUserProfileTargetName);
            }

            return Task.CompletedTask;
        }

        private static string SerializeUserProfile(UserProfile userProfile)
        {
            return JsonConvert.SerializeObject(userProfile, JsonSettings);
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

            var serializedUserProfile = LoadSecretByName(ZwiftUserProfileTargetName);
            UserProfile? userProfile = null;

            if (!string.IsNullOrEmpty(serializedUserProfile))
            {
                userProfile = JsonConvert.DeserializeObject<UserProfile>(serializedUserProfile, JsonSettings);
            }

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserProfile = userProfile
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
            catch (Win32Exception e) when (e.NativeErrorCode == 1168)
            {
                return null;
            }
        }
    }
}