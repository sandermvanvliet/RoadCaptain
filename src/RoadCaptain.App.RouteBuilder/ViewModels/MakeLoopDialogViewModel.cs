// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
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
            set
            {
                if (value == _noLoop) return;
                _noLoop = value;
                this.RaisePropertyChanged();
            }
        }

        public bool InfiniteLoop
        {
            get => _infiniteLoop;
            set
            {
                if (value == _infiniteLoop) return;
                _infiniteLoop = value;
                this.RaisePropertyChanged();
            }
        }

        public bool ConstrainedLoop
        {
            get => _constrainedLoop;
            set
            {
                if (value == _constrainedLoop) return;
                _constrainedLoop = value;
                this.RaisePropertyChanged();
            }
        }

        public int? NumberOfLoops
        {
            get => _numberOfLoops;
            set
            {
                if (value == _numberOfLoops) return;
                _numberOfLoops = value;
                this.RaisePropertyChanged();
            }
        }
        
        public ICommand CloseDialogCommand { get; }
    }
}

