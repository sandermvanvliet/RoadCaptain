// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using ReactiveUI;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class MakeLoopDialogViewModel : ViewModelBase
    {
        private bool _shouldMakeLoop;
        private LoopMode _loopMode;
        private int? _loopCount;

        public bool ShouldMakeLoop
        {
            get => _shouldMakeLoop;
            set
            {
                if (value == _shouldMakeLoop) return;
                _shouldMakeLoop = value;
                this.RaisePropertyChanged();
            }
        }

        public LoopMode LoopMode
        {
            get => _loopMode;
            set
            {
                if (value == _loopMode) return;
                _loopMode = value;
                this.RaisePropertyChanged();

                if (_loopMode == LoopMode.Infinite)
                {
                    LoopCount = null;
                }
            }
        }

        public int? LoopCount
        {
            get => _loopCount;
            set
            {
                if (value == _loopCount) return;
                _loopCount = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public class DesignTimeMakeLoopDialogViewModel : MakeLoopDialogViewModel
    {
    }
}

