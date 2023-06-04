﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class HttpRouteRepository : IRouteRepository
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRouteRepositorySettings _settings;
        private static readonly JsonSerializerSettings JsonSettings = new();
        private readonly JsonSerializer _serializer;
        private readonly RouteStoreToDisk _routeStoreToDisk;

        public HttpRouteRepository(HttpClient httpClient, HttpRouteRepositorySettings settings, RouteStoreToDisk routeStoreToDisk)
        {
            _httpClient = httpClient;
            _settings = settings;
            _routeStoreToDisk = routeStoreToDisk;
            _serializer = JsonSerializer.Create(JsonSettings);
            Name = _settings.Name;
        }

        public string Name { get; }

        public async Task<bool> IsAvailableAsync()
        {
            if (!_settings.IsValid)
            {
                return false;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_settings.Uri, "/2023-01/status"));

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<RouteModel[]> SearchAsync(string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            if (!_settings.IsValid)
            {
                throw new Exception("Route repository is not configured correctly");
            }

            var queryStringBuilder = new QueryStringBuilder();

            queryStringBuilder.AddIfNotDefault(nameof(creator), creator);
            queryStringBuilder.AddIfNotDefault(nameof(name), name);
            queryStringBuilder.AddIfNotDefault(nameof(zwiftRouteName), zwiftRouteName);
            queryStringBuilder.AddIfNotDefault(nameof(minDistance), minDistance);
            queryStringBuilder.AddIfNotDefault(nameof(maxDistance), maxDistance);
            queryStringBuilder.AddIfNotDefault(nameof(minAscent), minAscent);
            queryStringBuilder.AddIfNotDefault(nameof(maxAscent), maxAscent);
            queryStringBuilder.AddIfNotDefault(nameof(minDescent), minDescent);
            queryStringBuilder.AddIfNotDefault(nameof(maxDescent), maxDescent);
            queryStringBuilder.AddIfNotDefault(nameof(isLoop), isLoop);

            if (komSegments != null && komSegments.Any())
            {
                foreach (var segment in komSegments)
                {
                    queryStringBuilder.Add(nameof(komSegments), segment);
                }
            }

            if (sprintSegments != null && sprintSegments.Any())
            {
                foreach (var segment in sprintSegments)
                {
                    queryStringBuilder.Add(nameof(sprintSegments), segment);
                }
            }

            var builder = new UriBuilder
            {
                Scheme = _settings.Uri.Scheme,
                Host = _settings.Uri.Host,
                Port = _settings.Uri.Port,
                Path = "/2023-01/routes",
                Query = queryStringBuilder.ToString()
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to search for routes, received an non-successful response: {response.StatusCode.ToString()}");
            }
            
            using var textReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            await using var jsonTextReader = new JsonTextReader(textReader);
            var routeModels = _serializer.Deserialize<RouteModel[]>(jsonTextReader);

            if (routeModels != null)
            {
                return routeModels
                    .Select(r =>
                    {
                        r.RepositoryName = Name;
                        r.Uri = new Uri(_settings.Uri, $"2023-01/routes/{r.Id}");
                        r.PlannedRoute = UpgradeIfNecessaryAndSerialize(r.Serialized);
                        return r;
                    })
                    .ToArray();
            }

            return Array.Empty<RouteModel>();
        }

        private PlannedRoute? UpgradeIfNecessaryAndSerialize(string? routeModelSerialized)
        {
            if (routeModelSerialized == null)
            {
                return null;
            }

            try
            {
                return _routeStoreToDisk.DeserializeAndUpgrade(routeModelSerialized);
            }
            catch
            {
                return null;
            }
        }
    }
}