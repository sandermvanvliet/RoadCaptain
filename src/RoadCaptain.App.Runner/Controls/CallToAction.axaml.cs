using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Runner.ViewModels;

namespace RoadCaptain.App.Runner.Controls
{
    public partial class CallToAction : UserControl
    {
        public CallToAction()
        {
            InitializeComponent();
        }

        public CallToAction(CallToActionViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
