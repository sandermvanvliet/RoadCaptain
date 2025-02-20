// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class MakeLoopDialogViewModel : ViewModelBase
    {
        private bool _noLoop;
        private int? _numberOfLoops;
        private bool _infiniteLoop;
        private bool _constrainedLoop;

        public MakeLoopDialogViewModel()
        {
            CloseDialogCommand = new AsyncRelayCommand(
                    param => CloseDialog(param is DialogResult result ? result : DialogResult.Unknown),
                    _ => true)
                .OnSuccess(_ => ShouldClose?.Invoke(this, EventArgs.Empty));
        }

        private Task<CommandResult> CloseDialog(DialogResult dialogResult)
        {
            DialogResult = dialogResult;
            return Task.FromResult(CommandResult.Success());
        }

        public DialogResult DialogResult { get; set; }
        public event EventHandler? ShouldClose;
        
        public bool NoLoop
        {
            get => _noLoop;
            set => SetProperty(ref _noLoop, value);
        }

        public bool InfiniteLoop
        {
            get => _infiniteLoop;
            set => SetProperty(ref _infiniteLoop, value);
        }

        public bool ConstrainedLoop
        {
            get => _constrainedLoop;
            set => SetProperty(ref _constrainedLoop, value);
        }

        public int? NumberOfLoops
        {
            get => _numberOfLoops;
            set => SetProperty(ref _numberOfLoops, value);
        }
        
        public ICommand CloseDialogCommand { get; }
    }
}

