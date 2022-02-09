using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Extensions;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager
{
    internal class SceneTimer : BindableBase
    {
        private static bool _isStarted = false;
        private static TimeSpan _time;
        private static TimeSpan _startTime;
        private static TimeSpan _pauseTime;
        private static float _speed = 1f;

        public bool IsStarted
        {
            get => _isStarted;
            set
            {
                _isStarted = value;

                if (value)
                {
                    _startTime = TimeSpan.FromTicks(HighResolutionTimer.TimeGet() - (long)(_pauseTime.TotalSeconds * HighResolutionTimer.GetTimeFreq()));
                }
                else
                {
                    _pauseTime = _time;
                }

                OnPropertyChanged();
            }
        }
        public TimeSpan Time
        {
            get => _time;
            set
            {
                _time = value;

                if (!_isStarted)
                    _pauseTime = value;

                OnPropertyChanged();
            }
        }
        public TimeSpan StartTime => _startTime;
        public TimeSpan PauseTime { get => _pauseTime; set => _pauseTime = value; }
        public float Speed
        {
            get => _speed;
            set
            {
                if (_speed > 0.5f)
                    _speed = value;
            }
        }

        public void Start()
        {
            if (IsStarted)
                return;

            IsStarted = true;
        }
        public void Pause() => IsStarted = false;

        public void Reset()
        {
            Time = TimeSpan.Zero;
            _startTime = TimeSpan.FromTicks(HighResolutionTimer.TimeGet());
            _pauseTime = TimeSpan.Zero;
        }

        public void Tick()
        {
            if (!IsStarted)
                return;

            Time = Speed * TimeSpan.FromSeconds((HighResolutionTimer.TimeGet() - StartTime.Ticks) / (float)HighResolutionTimer.GetTimeFreq());
        }

        public void RaiseTimeChange()
        {
            OnPropertyChanged("Time");
        }
    }
}
