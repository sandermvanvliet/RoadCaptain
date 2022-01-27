using System.Net;
using System.Net.Http;
using System.Threading;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenConnectingToGame
    {
        private readonly TestableMessageHandler _handler;

        public WhenConnectingToGame()
        {
            _handler = new TestableMessageHandler();
            _handler
                .RespondTo()
                .Get()
                .ForUrl("/api/auth")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"realm\":\"zwift\",\"launcher\":\"https://launcher.zwift.com/launcher\",\"url\":\"https://secure.zwift.com/auth/\"}");

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

        [Fact]
        public void GivenUserNameAndPassword_RelayRequestIsSent()
        {
            var useCase = new ConnectToZwiftUseCase(
                new RequestTokenFromApi(new HttpClient(_handler)),
                new Zwift(new HttpClient(_handler)),
                new NopMonitoringEvents());

            useCase.ExecuteAsync(new ConnectCommand
                    {
                        Username = "test",
                        Password = "supersecret"
                    },
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            _handler
                .Requests
                .Should()
                .Contain(req => req.RequestUri.PathAndQuery == "/relay/profiles/me/phone");
        }
    }
}