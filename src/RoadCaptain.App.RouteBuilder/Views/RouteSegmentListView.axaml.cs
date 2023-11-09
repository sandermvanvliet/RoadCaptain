using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class RouteSegmentListView : UserControl
    {
        public RouteSegmentListView()
        {
            ViewModel = (DataContext as RouteSegmentListViewModel)!; // Suppressed because it's initialized from XAML
            
            InitializeComponent();
        }

        private RouteSegmentListViewModel ViewModel { get; }

        private void RouteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == ViewModel.Route.Last)
                {
                    // TODO: fixme
                    //ViewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.SelectedItem = RouteListView.Items.Cast<object>().Last();
                    }
                }
            }
        }
        
        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: fix me
            // if (e.AddedItems.Count == 1 && e.AddedItems[0] is SegmentSequenceViewModel viewModel && !string.IsNullOrEmpty(viewModel.SegmentId))
            // {
            //     ViewModel.HighlightSegment(viewModel.SegmentId);
            // }
            // else
            // {
            //     ViewModel.ClearSegmentHighlight();
            // }
        }
        
        private void MarkersOnRouteListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // TODO: fix me
            // if (e.AddedItems.Count == 1 && e.AddedItems[0] is MarkerViewModel viewModel)
            // {
            //     ViewModel.HighlightMarker(viewModel.Id);
            // }
            // else
            // {
            //     ViewModel.ClearMarkerHighlight();
            // }
        }
    }
}