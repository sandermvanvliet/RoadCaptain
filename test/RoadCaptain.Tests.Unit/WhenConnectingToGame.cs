// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenConnectingToGame
    {
        private readonly TestableMessageHandler _handler;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly ConnectToZwiftUseCase _useCase;

        public WhenConnectingToGame()
        {
            var monitoringEvents = new NopMonitoringEvents();
            _handler = new TestableMessageHandler();

            _gameStateDispatcher = new InMemoryGameStateDispatcher(monitoringEvents);
            _useCase = new ConnectToZwiftUseCase(new Zwift(new HttpClient(_handler)),
                monitoringEvents,
                new InMemoryGameStateDispatcher(monitoringEvents), // Use a different receiver than dispatcher
                _gameStateDispatcher);

            ConfigureSuccessfulResponses();
        }

        [Fact]
        public void GivenConnectionSecretIsNull_ArgumentExceptionIsThrown()
        {
            Action action = () => _useCase.ExecuteAsync(new ConnectCommand
                    {
                        AccessToken = "supersecret",
                        ConnectionEncryptionSecret = null
                    },
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Connection secret must be provided");
        }

        [Fact]
        public void GivenConnectionSecretIsEmptyArray_ArgumentExceptionIsThrown()
        {
            Action action = () => _useCase.ExecuteAsync(new ConnectCommand
                    {
                        AccessToken = "supersecret",
                        ConnectionEncryptionSecret = Array.Empty<byte>()
                    },
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Connection secret must be provided");
        }

        [Fact]
        public void GivenAccessToken_RelayRequestIsSent()
        {
            _useCase.ExecuteAsync(new ConnectCommand
                    {
                        AccessToken = "supersecret",
                        ConnectionEncryptionSecret = new byte[] { 0x1 }
                    },
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            _handler
                .Requests
                .Should()
                .Contain(req => req.RequestUri.PathAndQuery == "/relay/profiles/me/phone");
        }

        [Fact]
        public void GivenInvalidAccessToken_InvalidCredentialsStateIsDispatched()
        {
            _handler
                .RespondTo()
                .Get()
                .ForUrl("/api/servers")
                .With(HttpStatusCode.Forbidden);

            try
            {
                _useCase
                    .ExecuteAsync(new ConnectCommand
                        {
                            AccessToken = "supersecret",
                            ConnectionEncryptionSecret = new byte[] { 0x1 }
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // Nop
            }

            GetFirstDispatchedGameState()
                .Should()
                .BeOfType<InvalidCredentialsState>();
        }

        private GameStates.GameState GetFirstDispatchedGameState()
        {
            // This method is meant to collect the first game
            // state update that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first game state dispatch call without having
            // to do Thread.Sleep() calls.

            GameStates.GameState lastState = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no game state is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            _gameStateDispatcher.ReceiveGameState(
                gameState =>
                {
                    lastState = gameState;

                    // Cancel after the first state is dispatched.
                    tokenSource.Cancel();
                });

            // This call blocks until the callback is invoked or
            // the cancellation token expires automatically.
            _gameStateDispatcher.Start(tokenSource.Token);

            return lastState;
        }

        private void ConfigureSuccessfulResponses()
        {
            _handler
                .RespondTo()
                .Get()
                .ForUrl("/api/auth")
                .With(HttpStatusCode.OK)
                .AndContent("application/json",
                    "{\"realm\":\"zwift\",\"launcher\":\"https://launcher.zwift.com/launcher\",\"url\":\"https://secure.zwift.com/auth/\"}");

            _handler
                .RespondTo()
                .Post()
                .ForUrl("/auth/realms/zwift/protocol/openid-connect/token")
                .With(HttpStatusCode.OK)
                .AndContent("application/json",
                    "{\r\n    \"access_token\": \"SECRET\",\r\n    \"expires_in\": 21600,\r\n    \"refresh_expires_in\": 691200,\r\n    \"refresh_token\": \"SECRET\",\r\n    \"token_type\": \"bearer\",\r\n    \"not-before-policy\": 1408478984,\r\n    \"session_state\": \"85bca556-eff7-4304-b947-28c810d7f564\",\r\n    \"scope\": \"\"\r\n}");

            _handler
                .RespondTo()
                .Get()
                .ForUrl("/api/servers")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"baseUrl\":\"https://us-or-rly101.zwift.com/relay\"}");

            _handler
                .RespondTo()
                .Put()
                .ForUrl("/relay/profiles/me/phone")
                .With(HttpStatusCode.NoContent);

            _handler
                .RespondTo()
                .Get()
                .ForUrl("/api/profiles/me")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"id\":12345,\"riding\":true,\"likelyInGame\":true}");
        }
    }
}