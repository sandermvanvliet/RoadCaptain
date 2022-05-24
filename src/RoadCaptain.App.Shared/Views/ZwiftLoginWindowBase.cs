using Avalonia.Controls;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared.Views
{
    public abstract class ZwiftLoginWindowBase : Window
    {
        public TokenResponse? TokenResponse { get; set; }
    }
}