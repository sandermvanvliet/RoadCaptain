// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class HttpRouteRepository : IRouteRepository
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRouteRepositorySettings _settings;
        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };

        private readonly JsonSerializer _serializer;
        private readonly RouteStoreToDisk _routeStoreToDisk;
        private readonly ISecurityTokenProvider _securityTokenProvider;

        public HttpRouteRepository(
            HttpClient httpClient, 
            HttpRouteRepositorySettings settings, 
            RouteStoreToDisk routeStoreToDisk,
            ISecurityTokenProvider securityTokenProvider)
        {
            _httpClient = httpClient;
            _settings = settings;
            _routeStoreToDisk = routeStoreToDisk;
            _securityTokenProvider = securityTokenProvider;
            _serializer = JsonSerializer.Create(JsonSettings);
            Name = _settings.Name;
            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions())
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions())
                .Build();
        }

        public string Name { get; }
        public bool IsReadOnly => false;
        public bool RequiresAuthentication => true;
        
        private readonly ResiliencePipeline _resiliencePipeline;

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            if (!_settings.IsValid)
            {
                return false;
            }

            return await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_settings.Uri, "/2023-01/status"));
                using var response = await _httpClient.SendAsync(request, ct);

                return response.IsSuccessStatusCode;
            }, cancellationToken);
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
            string[]? sprintSegments = null,
            CancellationToken cancellationToken = default)
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

            var securityToken = await _securityTokenProvider.GetSecurityTokenForPurposeAsync(
                TokenPurpose.RouteRepositoryAccess, 
                TokenPromptBehaviour.DoNotPrompt);
            
            using var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            
            // We don't require one, but it's nice to have one
            if (!string.IsNullOrEmpty(securityToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", securityToken);
            }

            using var response = await _resiliencePipeline.ExecuteAsync(async ct => await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseContentRead,
                ct), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to search for routes, received an non-successful response: {response.StatusCode}");
            }

            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(TimeSpan.FromSeconds(3));
            
            using var textReader = new StreamReader(await response.Content.ReadAsStreamAsync(tokenSource.Token));
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
                        r.World = r.PlannedRoute?.WorldId;
                        r.IsReadOnly = IsReadOnly;
                        return r;
                    })
                    .ToArray();
            }

            return [];
        }

        public async Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, Uri? routeUri)
        {
            var token = await _securityTokenProvider.GetSecurityTokenForPurposeAsync(TokenPurpose.RouteRepositoryAccess, TokenPromptBehaviour.Prompt);
            
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("A valid token is required for this route repository");
            }

            var createRouteModel = new CreateRouteModel(plannedRoute);

            HttpMethod httpMethod;
            
            if (routeUri == null)
            {
                httpMethod = HttpMethod.Post;
                
                var builder = new UriBuilder
                {
                    Scheme = _settings.Uri.Scheme,
                    Host = _settings.Uri.Host,
                    Port = _settings.Uri.Port,
                    Path = "/2023-01/routes"
                };
                
                routeUri = builder.Uri;
            }
            else
            {
                httpMethod = HttpMethod.Put;
            }

            using var request = new HttpRequestMessage(httpMethod, routeUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(createRouteModel, JsonSettings), Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var response = await _resiliencePipeline.ExecuteAsync(async ct => await _httpClient.SendAsync(request, ct), tokenSource.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("You're not authorized to store a route on this repository");
                }
                
                throw new Exception($"Unable to store route, received an non-successful response: {response.StatusCode}");
            }
            
            using var textReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            await using var jsonTextReader = new JsonTextReader(textReader);
            var routeModel = _serializer.Deserialize<RouteModel>(jsonTextReader);

            if (routeModel == null)
            {
                throw new Exception("Unable to store route, response was empty");
            }
            
            return routeModel;
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
        
        public async Task DeleteAsync(Uri routeUri)
        {
            var securityToken = await _securityTokenProvider.GetSecurityTokenForPurposeAsync(TokenPurpose.RouteRepositoryAccess, TokenPromptBehaviour.Prompt);
            
            if (string.IsNullOrEmpty(securityToken))
            {
                throw new ArgumentException("A security token is required to delete a route");
            }

            using var request = new HttpRequestMessage(HttpMethod.Delete, routeUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", securityToken);

            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                
            using var response = await _resiliencePipeline.ExecuteAsync(async ct => await _httpClient.SendAsync(request, ct), tokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                var message = $"HTTP error {(int)response.StatusCode} {response.ReasonPhrase}";
                
                throw new Exception($"Unable to delete route: {message}");
            }
        }
    }
}
