// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Reflection;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;
using WebViewControl;
using Xilium.CefGlue;

namespace RoadCaptain.App.MacOs.Views
{
    public partial class ZwiftLoginWindow : ZwiftLoginWindowBase
    {
        private readonly FieldInfo _cefRequestField;
        private bool _isInitialActivation = true;

        public ZwiftLoginWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var zwiftAuthView = this.Find<WebView>("ZwiftAuthView");
            zwiftAuthView.BeforeNavigate += ZwiftAuthViewOnBeforeNavigate;
            zwiftAuthView.AllowDeveloperTools = false;
            zwiftAuthView.DisableBuiltinContextMenus = true;
            zwiftAuthView.BeforeResourceLoad += ZwiftAuthViewOnBeforeResourceLoad;

            var fields = typeof(Request)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .ToList();

            _cefRequestField = fields.Single(f => f.FieldType == typeof(CefRequest));
        }

        private void ZwiftAuthViewOnBeforeResourceLoad(ResourceHandler resourcehandler)
        {
            if (resourcehandler.Url == "https://www.zwift.com/auth/login")
            {
                // Get the underlying request from the resource handler,
                // need to use reflection because it isn't exposed by 
                // the webview control...
                var cefRequest = _cefRequestField.GetValue(resourcehandler) as CefRequest;
                var postData = Encoding.UTF8.GetString(cefRequest.PostData.GetElements()[0].GetBytes());
                
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, resourcehandler.Url)
                {
                    Content = new StringContent(postData, Encoding.UTF8, "application/json")
                };

                CopyHeadersToRequest(cefRequest, requestMessage);
                
                using var client = new HttpClient();
                var response = client.Send(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var serialized = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    TokenResponse = JsonSerializer.Deserialize<TokenResponse>(serialized) ?? new TokenResponse();

                    if (serialized.Contains("access_token"))
                    {
                        var snakeCaseTokenResponse = JsonSerializer.Deserialize<TokenResponseSnakeCase>(serialized) ??
                                                     new TokenResponseSnakeCase();
                        TokenResponse.AccessToken = snakeCaseTokenResponse.AccessToken;
                        TokenResponse.RefreshToken = snakeCaseTokenResponse.RefreshToken;
                    }

                    // We were successful
                    Dispatcher.UIThread.InvokeAsync(() => Close(true));

                    // Let the browser continue normally even though it will be stopped later
                    resourcehandler.RespondWith(new MemoryStream(Encoding.UTF8.GetBytes(serialized)));
                }
            }
        }

        private static void CopyHeadersToRequest(CefRequest cefRequest, HttpRequestMessage request)
        {
            var headerKeys = cefRequest
                .GetHeaderMap()
                .AllKeys
                .Where(key => !"Content-Type".Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (var key in headerKeys)
            {
                request.Headers.Add(key, cefRequest.GetHeaderByName(key));
            }
        }

        private void ZwiftAuthViewOnBeforeNavigate(Request request)
        {
            if (request.Url.StartsWith("https://www.zwift.com/feed"))
            {
                request.Cancel();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            if (_isInitialActivation)
            {
                _isInitialActivation = false;

                var webView = this.Find<WebView>("ZwiftAuthView");
                
                webView.LoadUrl(
                    "https://www.zwift.com/eu/sign-in?redirect_uri=https://www.zwift.com/feed?auth_redirect=true");
            }
        }
    }
}
