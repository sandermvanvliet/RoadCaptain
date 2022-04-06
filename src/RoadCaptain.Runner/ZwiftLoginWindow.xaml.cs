// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner
{
    /// <summary>
    ///     Interaction logic for ZwiftLoginWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ZwiftLoginWindow : Window
    {
        private bool _isInitialActivation = true;

        public ZwiftLoginWindow()
        {
            InitializeComponent();

            // This needs to be set to a user writeable path 
            // otherwise the web view tries to initialize its
            // temp folder under Program Files which is not
            // accessible.
            ZwiftAuthView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = Path.GetTempPath(),
            };
        }

        public TokenResponse TokenResponse { get; set; }

        private void ZwiftAuthView_OnCoreWebView2InitializationCompleted(object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            ZwiftAuthView.CoreWebView2.WebResourceResponseReceived += ZwiftAuthView_WebResourceResponseReceived;
            ZwiftAuthView.CoreWebView2.CookieManager.DeleteAllCookies();
        }

        private void ZwiftAuthView_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://www.zwift.com/feed"))
            {
                e.Cancel = true;
            }
        }

        private async void ZwiftAuthView_WebResourceResponseReceived(
            object sender,
            CoreWebView2WebResourceResponseReceivedEventArgs e)
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
                    DialogResult = true;
                    
                    Close();
                }
                catch
                {
                    // nop
                }
            }
        }

        private void ZwiftLoginWindow_OnActivated(object sender, EventArgs e)
        {
            if (_isInitialActivation)
            {
                _isInitialActivation = false;

                ZwiftAuthView.Source =
                    new Uri(
                        "https://www.zwift.com/eu/sign-in?redirect_uri=https://www.zwift.com/feed?auth_redirect=true");
            }
        }
    }
}
