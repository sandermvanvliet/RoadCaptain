// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Shared.Dialogs
{
    public partial class AboutRoadCaptainDialog : Window
    {
        public AboutRoadCaptainDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new AboutRoadCaptainViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}
