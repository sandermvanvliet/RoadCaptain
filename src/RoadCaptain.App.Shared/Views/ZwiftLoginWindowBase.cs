// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia.Controls;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared.Views
{
    public abstract class ZwiftLoginWindowBase : Window
    {
        public TokenResponse? TokenResponse { get; set; }
    }
}
