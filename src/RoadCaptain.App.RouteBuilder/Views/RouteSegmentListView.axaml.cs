// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using RoadCaptain.App.RouteBuilder.ViewModels;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;
using ListBox = Avalonia.Controls.ListBox;
using UserControl = Avalonia.Controls.UserControl;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class RouteSegmentListView : UserControl
    {
        public static readonly StyledProperty<Segment?> SelectedSegmentProperty =
            AvaloniaProperty.Register<RouteSegmentListView, Segment?>(nameof(SelectedSegment));

        public static readonly StyledProperty<string?> SelectedMarkerIdProperty =
            AvaloniaProperty.Register<RouteSegmentListView, string?>(nameof(SelectedMarkerId));

        private RouteSegmentListViewModel? _viewModel;

        public RouteSegmentListView()
        {
            InitializeComponent();

            DataContextChanged += (_, _) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelOnPropertyChanged;
                    _viewModel = null;
                }

                if (DataContext is not RouteSegmentListViewModel viewModel) return;
                    
                _viewModel = viewModel;
                _viewModel.PropertyChanged += OnViewModelOnPropertyChanged;
            };
        }

        private void OnViewModelOnPropertyChanged(object? _, PropertyChangedEventArgs propChangedArgs)
        {
            switch (propChangedArgs.PropertyName)
            {
                case nameof(_viewModel.SelectedSegmentSequence):
                    SelectedSegment = _viewModel!.SelectedSegmentSequence?.Segment;
                    break;
                case nameof(_viewModel.SelectedMarker):
                    SelectedMarkerId = _viewModel!.SelectedMarker?.Id;
                    break;
            }
        }

        public Segment? SelectedSegment
        {
            get => GetValue(SelectedSegmentProperty);
            set => SetValue(SelectedSegmentProperty, value);
        }

        public string? SelectedMarkerId
        {
            get => GetValue(SelectedMarkerIdProperty);
            set => SetValue(SelectedMarkerIdProperty, value);
        }

        private void RouteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == _viewModel!.Route.Last)
                {
                    _viewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.SelectedItem = RouteListView.Items.Cast<object>().Last();
                        RouteListView.Focus();
                    }
                }
            }
        }
    }
}
