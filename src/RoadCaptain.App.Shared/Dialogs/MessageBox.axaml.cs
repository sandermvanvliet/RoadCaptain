// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Shared.Dialogs
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            var tagValue = (sender as Button)?.Tag as string;

            if (string.IsNullOrEmpty(tagValue) ||
                "Default".Equals(tagValue, StringComparison.InvariantCultureIgnoreCase))
            {
                Close(MessageBoxResult.None);
            }
            else
            {
                var result = Enum.Parse<MessageBoxResult>(tagValue);

                Close(result);
            }
        }

        public static async Task<MessageBoxResult> ShowAsync(
            string message,
            string title,
            MessageBoxButton buttons,
            Window? owner, 
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            var messageBox = new MessageBox
            {
                DataContext = new MessageBoxViewModel(buttons, title, message, icon)
            };

            return await messageBox.ShowDialog<MessageBoxResult>(owner);
        }
    }
}
