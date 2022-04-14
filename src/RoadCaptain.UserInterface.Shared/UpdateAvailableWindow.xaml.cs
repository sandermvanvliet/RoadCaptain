using System.Windows;

namespace RoadCaptain.UserInterface.Shared
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class UpdateAvailableWindow : Window
    {
        public UpdateAvailableWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
