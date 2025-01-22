// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class WhenLoadingRoute
    {
        [Fact]
        public void GivenRoutePathIsNull_ArgumentExceptionIsThrown()
        {
            var action = () => LoadRoute(null);

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Route path must be a valid path (Parameter 'command')");
        }

        [Fact]
        public void GivenRouteExists_RouteSelectedEventIsDispatched()
        {
            LoadRoute(ExistingRoutePath);

            GetDispatchedRoute()
                .Should()
                .NotBeNull();
        }

        private readonly LoadRouteUseCase _useCase;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private const string ExistingRoutePath = "someroute.json";

        public WhenLoadingRoute()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents(), new StubPathProvider());
            IRouteStore routeStore = new StubRouteStore();

            _useCase = new LoadRouteUseCase(
                _gameStateDispatcher,
                routeStore);
        }

        private void LoadRoute(string? routePath)
        {
            _useCase.Execute(new LoadRouteCommand { Path = routePath });
        }

        private PlannedRoute? GetDispatchedRoute()
        {
            // This method is meant to collect the first route
            // that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first dispatch call without having
            // to do Thread.Sleep() calls.

            PlannedRoute? lastRoute = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no route is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            _gameStateDispatcher.ReceiveRoute(
                route =>
                {
                    lastRoute = route;

                    // Cancel after the first route is dispatched.
                    tokenSource.Cancel();
                });

            // This call blocks until the callback is invoked or
            // the cancellation token expires automatically.
            _gameStateDispatcher.Start(tokenSource.Token);

            return lastRoute;
        }
    }
}
