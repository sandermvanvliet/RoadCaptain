// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Microsoft.AspNetCore.Http.Extensions;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ILogger = Serilog.ILogger;

namespace RoadCaptain.App.Web
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private const string Pattern = "([A-Za-z0-9+_%\\.\\-]*@[A-Za-z0-9+_%\\.\\-]*)";
        private const string UrlEncodedPattern = "([A-Za-z0-9+_%\\.\\-]*%40[A-Za-z0-9+_%\\.\\-]*)";
        private static readonly Regex EmailRegex = new Regex($"([A-Za-z0-9])={Pattern}", RegexOptions.Compiled);
        private static readonly Regex UrlEncodedEmailRegex = new Regex($"([A-Za-z0-9])={UrlEncodedPattern}", RegexOptions.Compiled);

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var pathAndQuery = SanitizeUrl(httpContext.Request);

            _logger.Information(
                "Request start: {method} {url}",
                httpContext.Request.Method,
                pathAndQuery);

            int? statusCode = null;
            var duration = Stopwatch.StartNew();

            try
            {
                await _next.Invoke(httpContext);
                statusCode = httpContext.Response.StatusCode;
            }
            catch
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                throw;
            }
            finally
            {
                _logger.Information(
                    "Request end: {status_code} {duration} {method} {url}",
                    statusCode ?? (int)HttpStatusCode.InternalServerError,
                    duration.ElapsedMilliseconds,
                    httpContext.Request.Method,
                    pathAndQuery);
            }
        }

        private static string SanitizeUrl(HttpRequest request)
        {
            var sanitizedUrl = request.GetEncodedPathAndQuery();

            try
            {
                sanitizedUrl = EmailRegex.Replace(sanitizedUrl, m => m.Groups[1].Value +  "=MASKED");
            }
            catch 
            {
                // Nop
            }
            
            try
            {
                sanitizedUrl = UrlEncodedEmailRegex.Replace(sanitizedUrl, m => m.Groups[1].Value +  "=MASKED");
            }
            catch 
            {
                // Nop
            }

            return sanitizedUrl;
        }
    }
}
