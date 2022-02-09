using System;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal class KeyframeTranslator
    {
        private int _secondLengthInPixels;
        private Keyframe _keyframe;
        private TimeSpan _fixedTime = TimeSpan.Zero;

        private Point _fixedPointerPosition;

        private DispatcherTimer _boosterTimer;
        private TimeSpan _boosterTimerInterval = new TimeSpan(0, 0, 0, 0, 1);
        private KeyframeBoostingDirection _boosterDirection;
        private DateTimeOffset _boosterStartTime;
        private int _boosterSpeed = 3;

        private bool _needPointerPositionReset = false;

        public Keyframe Keyframe => _keyframe;
        public bool IsActive => _keyframe.IsEmpty == false;
        public bool IsBoosterActive => _boosterTimer.IsEnabled;
        public bool IsMovedByOffset;

        public KeyframeTranslator(int secondLengthInPixels)
        {
            _secondLengthInPixels = secondLengthInPixels;

            _boosterTimer = new DispatcherTimer();
            _boosterTimer.Interval = _boosterTimerInterval;
            _boosterTimer.Tick += BoosterTimer_Tick;
        }

        #region Main Logic

        public bool TryActivate(Keyframe keyframe, Point pointerPosition)
        {
            if (IsActive || keyframe.IsEmpty)
                return false;

            _keyframe = keyframe;
            _fixedTime = keyframe.Point.Time;

            _fixedPointerPosition = pointerPosition;

            return true;
        }

        public bool TryTranslate(Point pointerPosition)
        {
            if (IsActive == false)
                return false;

            if (_needPointerPositionReset)
            {
                _fixedPointerPosition = pointerPosition;
                _needPointerPositionReset = false;
                _fixedTime = _keyframe.Point.Time;
            }

            var deltaX = pointerPosition.X - _fixedPointerPosition.X;
            var deltaTime = GetTimeByPixels(deltaX);

            AddTime(deltaTime);

            IsMovedByOffset = false;

            return true;
        }

        public bool TryMoveByOffset(int offset)
        {
            if (IsActive == false)
                return false;

            _needPointerPositionReset = true;
            _fixedTime = _keyframe.Point.Time;

            var deltaTime = GetTimeByPixels(offset);

            AddTime(deltaTime);

            IsMovedByOffset = true;

            return true;
        }

        private void AddTime(TimeSpan time)
        {
            var resultTime = _fixedTime + time;

            var clampedTimeSeconds = Math.Clamp(resultTime.TotalSeconds, Keyframe.MinTime.TotalSeconds, Keyframe.MaxTime.TotalSeconds);

            _keyframe.Point.Time = TimeSpan.FromSeconds(clampedTimeSeconds);
        }

        public void Deactivate()
        {
            _keyframe = Keyframe.Empty;
            _fixedTime = TimeSpan.Zero;
            _fixedPointerPosition = new Point();

            IsMovedByOffset = false;

            DeactivateBooster();
        }

        #endregion

        #region Booster Logic

        public bool TryActivateBooster(KeyframeBoostingDirection direction)
        {
            if (IsActive == false || direction == KeyframeBoostingDirection.None)
                return false;

            _fixedTime = Keyframe.Point.Time;

            _boosterDirection = direction;
            _boosterStartTime = DateTimeOffset.Now;
            _boosterTimer.Start();

            return true;
        }

        public void DeactivateBooster()
        {
            if (_boosterTimer.IsEnabled == false)
                return;

            _needPointerPositionReset = true;

            _fixedTime = Keyframe.Point?.Time ?? TimeSpan.Zero;
            _boosterTimer.Stop();
            _boosterDirection = KeyframeBoostingDirection.None;
        }

        private void BoosterTimer_Tick(object sender, object e)
        {
            var time = _boosterStartTime - DateTimeOffset.Now;

            if (_boosterDirection == KeyframeBoostingDirection.Left)
                AddTime(time * _boosterSpeed);
            else if (_boosterDirection == KeyframeBoostingDirection.Right)
                AddTime(-time * _boosterSpeed);
        }

        #endregion

        #region Set and Get Methods

        public void SetSecondLength(int secondLengthInPixels)
            => _secondLengthInPixels = secondLengthInPixels;

        private TimeSpan GetTimeByPixels(double pixels)
            => TimeSpan.FromSeconds(pixels / _secondLengthInPixels);

        private double GetPixelsByTime(TimeSpan time)
            => time.TotalSeconds * _secondLengthInPixels;

        #endregion
    }
}
