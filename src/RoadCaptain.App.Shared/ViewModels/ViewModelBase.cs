// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/


using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoadCaptain.App.Shared.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

