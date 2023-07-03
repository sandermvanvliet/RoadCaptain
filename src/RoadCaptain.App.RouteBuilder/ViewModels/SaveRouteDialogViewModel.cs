// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.IdentityModel.JsonWebTokens;
using ReactiveUI;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    internal class SaveRouteDialogViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private RouteViewModel _route;
        private ImmutableList<string>? _repositories;
        private string? _selectedRepository;
        private readonly RetrieveRepositoryNamesUseCase _retrieveRepositoryNamesUseCase;
        private readonly SaveRouteUseCase _saveRouteUseCase;
        private readonly IZwiftCredentialCache _credentialCache;
        private readonly IZwift _zwift;

        public SaveRouteDialogViewModel(
            IWindowService windowService,
            RouteViewModel route,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase, 
            SaveRouteUseCase saveRouteUseCase, IZwiftCredentialCache credentialCache, IZwift zwift)
        {
            _windowService = windowService;
            _route = route;
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
            _saveRouteUseCase = saveRouteUseCase;
            _credentialCache = credentialCache;
            _zwift = zwift;
        }

        public ICommand SaveRouteCommand => new AsyncRelayCommand(
                _ => SaveRoute(),
                _ => SelectedRepository != null && !string.IsNullOrEmpty(RouteName))
            .OnSuccess(async _ =>
            {
                await CloseWindow();
            })
            .OnFailure(async result =>
                await _windowService.ShowErrorDialog($"Unable to save route: {result.Message}", null))
            .SubscribeTo(this, () => SelectedRepository)
            .SubscribeTo(this, () => RouteName);
        
        private async Task<CommandResult> SaveRoute()
        {
            if (string.IsNullOrEmpty(RouteName))
            {
                return CommandResult.Failure("Route name is empty");
            }
            if (string.IsNullOrEmpty(SelectedRepository))
            {
                return CommandResult.Failure("No route repository selected");
            }
            
            try
            {
                var token = await AuthenticateToZwiftAsync();

                // TODO: Handle situation where the token has expired.
                
                await _saveRouteUseCase.ExecuteAsync(new SaveRouteCommand(_route.AsPlannedRoute()!, RouteName, SelectedRepository, token?.AccessToken));
                
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
        }

        private async Task<TokenResponse?> AuthenticateToZwiftAsync()
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

            if (tokenResponse != null)
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

        public string? RouteName
        {
            get => _route.Name;
            set
            {
                if (_route.Name == value) return;
                _route.Name = value ?? string.Empty;
                this.RaisePropertyChanged();
            }
        }

        public RouteViewModel Route
        {
            get => _route;
            set
            {
                if (_route == value) return;
                _route = value;
                this.RaisePropertyChanged();
            }
        }

        public ImmutableList<string> Repositories
        {
            get => _repositories ?? ImmutableList<string>.Empty;
            set
            {
                if (value == _repositories)
                {
                    return;
                }
                
                _repositories = value;
                
                this.RaisePropertyChanged();
            }
        }

        public string? SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (value == _selectedRepository)
                {
                    return;
                }
                
                _selectedRepository = value;
                
                this.RaisePropertyChanged();
            }
        }
        public event EventHandler? ShouldClose;

        private Task CloseWindow()
        {
            ShouldClose?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public void Initialize()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute(new RetrieveRepositoryNameCommand(RetrieveRepositoriesIntent.Store)).ToImmutableList();
        }
    }
}
