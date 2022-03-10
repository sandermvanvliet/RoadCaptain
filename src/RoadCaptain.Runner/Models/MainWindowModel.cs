using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoadCaptain.Runner.Annotations;

namespace RoadCaptain.Runner.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string _windowTitle;
        private double _elapsedDistance;
        private double _elapsedAscent;
        private double _elapsedDescent;
        private SegmentSequenceModel _currentSegment;
        private SegmentSequenceModel _nextSegment;
        private int _currentSegmentSequenceNumber;

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (value == _windowTitle)
                {
                    return;
                }

                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public PlannedRoute Route { get; set; }

        public double ElapsedDistance
        {
            get => _elapsedDistance;
            set
            {
                if (value.Equals(_elapsedDistance)) return;
                _elapsedDistance = value;
                OnPropertyChanged();
            }
        }

        public double ElapsedAscent
        {
            get => _elapsedAscent;
            set
            {
                if (value.Equals(_elapsedAscent)) return;
                _elapsedAscent = value;
                OnPropertyChanged();
            }
        }

        public double ElapsedDescent
        {
            get => _elapsedDescent;
            set
            {
                if (value.Equals(_elapsedDescent)) return;
                _elapsedDescent = value;
                OnPropertyChanged();
            }
        }

        public SegmentSequenceModel CurrentSegment
        {
            get => _currentSegment;
            set
            {
                if (Equals(value, _currentSegment)) return;
                _currentSegment = value;
                OnPropertyChanged();
            }
        }

        public SegmentSequenceModel NextSegment
        {
            get => _nextSegment;
            set
            {
                if (Equals(value, _nextSegment)) return;
                _nextSegment = value;
                OnPropertyChanged();
            }
        }

        public int CurrentSegmentSequenceNumber
        {
            get => _currentSegmentSequenceNumber;
            set
            {
                if (value == _currentSegmentSequenceNumber) return;
                _currentSegmentSequenceNumber = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SegmentSequenceModel
    {
    }
}