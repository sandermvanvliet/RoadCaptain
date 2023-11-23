// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class RouteSegmentListView : UserControl
    {
        public RouteSegmentListView()
        {
            InitializeComponent();
        }

        private RouteSegmentListViewModel ViewModel
        {
            get
            {
                if (DataContext is RouteSegmentListViewModel viewModel)
                {
                    return viewModel;
                }

                throw new Exception(
                    "DataContext hasn't been initialized correctly, expected a RouteSegmentListViewModel but got null or something totally different");
            }
        }

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
