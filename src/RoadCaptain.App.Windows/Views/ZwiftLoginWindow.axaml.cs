// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Web.WebView2.Core;
using RoadCaptain.App.Runner.Controls;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.Windows.Views
{
    public partial class ZwiftLoginWindow : ZwiftLoginWindowBase
    {
        private readonly WebView _webView;
        private bool _isInitialActivation = true;

        public ZwiftLoginWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _webView = this.Find<WebView>("WebViewMain");
            _webView.CoreWebView2InitializationCompleted += ZwiftAuthView_OnCoreWebView2InitializationCompleted;
            _webView.NavigationStarting += ZwiftAuthView_OnNavigationStarting;
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

                _webView.Navigate("https://www.zwift.com/eu/sign-in?redirect_uri=https://www.zwift.com/feed?auth_redirect=true");
            }
        }
        
        private void ZwiftAuthView_OnCoreWebView2InitializationCompleted(object? sender,
            CoreWebView2InitializationCompletedEventArgs? e)
        {
            _webView.CoreWebView2.WebResourceResponseReceived += ZwiftAuthView_WebResourceResponseReceived;
            _webView.CoreWebView2.CookieManager.DeleteAllCookies();
        }

        private void ZwiftAuthView_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs? e)
        {
            if (e.Uri.StartsWith("https://www.zwift.com/feed"))
            {
                e.Cancel = true;
            }
        }

        private async void ZwiftAuthView_WebResourceResponseReceived(
            object? sender,
            CoreWebView2WebResourceResponseReceivedEventArgs? e)
        {
            if (e.Request.Uri.StartsWith("https://www.zwift.com/auth/login") &&
                "POST".Equals(e.Request.Method, StringComparison.InvariantCultureIgnoreCase) &&
                e.Response.StatusCode == 200)
            {
                try
                {
                    // This is the callback that contains the tokens
                    var stream = await e.Response.GetContentAsync();

                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    
                    TokenResponse = JsonSerializer.Deserialize<TokenResponse>(content) ?? new TokenResponse();

                    if (content.Contains("access_token"))
                    {
                        var snakeCaseTokenResponse = JsonSerializer.Deserialize<TokenResponseSnakeCase>(content) ?? new TokenResponseSnakeCase();
                        TokenResponse.AccessToken = snakeCaseTokenResponse.AccessToken;
                        TokenResponse.RefreshToken = snakeCaseTokenResponse.RefreshToken;
                    }

                    // We were successful
                    Close(true);
                }
                catch
                {
                    // nop
                }
            }
        }
    }
}

