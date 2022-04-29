using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Shared.Dialogs
{
    public class MessageBox : Window
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
            var tagValue = (sender as Button).Tag as string;

            var result = Enum.Parse<MessageBoxResult>(tagValue);
            
            Close(result);
        }

        public static async Task<MessageBoxResult> ShowAsync(
            string message,
            string title,
            MessageBoxButton buttons,
            Window owner, 
            MessageBoxImage image = MessageBoxImage.Information)
        {
            var messageBox = new MessageBox
            {
                DataContext = new MessageBoxViewModel(buttons, title, message)
            };

            return await messageBox.ShowDialog<MessageBoxResult>(owner);
        }
    }
}