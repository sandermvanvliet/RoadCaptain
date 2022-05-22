using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace RoadCaptain.App.Runner.Controls
{
    internal class WebView : NativeControlHost
    {
        private WebView2? _webView;
        public CoreWebView2 CoreWebView2 => _webView.CoreWebView2;

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (OperatingSystem.IsWindows())
            {
                _webView = new WebView2();
                _webView.CreationProperties = new CoreWebView2CreationProperties
                {
                    UserDataFolder = Path.GetTempPath(),
                };
                _webView.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
                _webView.NavigationStarting += OnNavigationStarting;

                return new PlatformHandle(_webView.Handle, "HWND");
            }

            return base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (OperatingSystem.IsWindows())
            {
                _webView?.Dispose();
                _webView = null;
            }
            else
            {
                base.DestroyNativeControlCore(control);
            }
        }

        public event EventHandler<CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
        public event EventHandler<CoreWebView2InitializationCompletedEventArgs>? CoreWebView2InitializationCompleted;

        private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            NavigationStarting?.Invoke(sender, e);
        }

        private void OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            CoreWebView2InitializationCompleted?.Invoke(sender, e);
        }

        public void Navigate(string url)
        {
            _webView.Source = new Uri(url);
        }
    }
}
