using Microsoft.Graphics.Canvas;
using System;
using Windows.UI.Xaml;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Teeditor_Direct3DInterop;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.ViewModels
{
    internal class EditorViewModel : BindableBase
    {
        private ITab _currentTab;

        //private Graphics _graphics;
        private InputComponent _inputComponent;

        private int _collectCount = 0; // remove after debug

        private bool _isIdle = true;

        private Action UpdateTabAction;
        private Action UpdateSizeAction;

        private event EventHandler FrameStarted;

        public event TabUpdateHandler TabUpdated;
        public delegate void TabUpdateHandler(object sender, ITab newTab);

        public event ActionOnGameLoopRunnedHandler ActionOnGameLoopRunned;
        public delegate void ActionOnGameLoopRunnedHandler(object sender, Action action);

        public ITab CurrentTab
        {
            get => _currentTab;
            set => Set(ref _currentTab, value);
        }

        public EditorViewModel()
        {
            FrameStarted += CanvasViewModel_FrameStarted;
        }

        #region Methods

        #region Init methods

        public void SetTab(ITab tab)
        {
            UpdateTabAction = new Action(() =>
            {
                if (CurrentTab != null)
                {
                    CurrentTab.SceneManager.CommandExecuted -= SceneManager_CommandExecuted;
                    CurrentTab = null;
                }

                if (tab == null || tab.SceneManager == null)
                {
                    _isIdle = true;
                    return;
                }

                var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentTab = tab;
                    CurrentTab.SceneManager.CommandExecuted += SceneManager_CommandExecuted;
                    CurrentTab.SceneManager.SetSize(CanvasProperties.Size);

                    _isIdle = false;
                });

                //CurrentTab = tab;
                //CurrentTab.SceneManager.CommandExecuted += SceneManager_CommandExecuted;
                //CurrentTab.SceneManager.SetSize(CanvasProperties.ScreenSize);

                //_isIdle = false;
                //CurrentTab.Data.Loaded += CurrentTabData_ContentLoaded;
            });
        }

        //private void CurrentTabData_ContentLoaded(object sender, EventArgs e)
        //{
        //    _isIdle = false;

        //    var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        TabUpdated?.Invoke(this, CurrentTab);
        //    });

        //    CurrentTab.Data.Loaded -= CurrentTabData_ContentLoaded;
        //}

        private void SceneManager_CommandExecuted(object sender, IUndoRedoableCommand command)
        {
            if (_isIdle)
                return;

            command.Execute(CurrentTab.CommandManager);
        }

        private void CanvasViewModel_FrameStarted(object sender, EventArgs args)
        {
            UpdateTabAction?.Invoke();
            UpdateTabAction = null;

            UpdateSizeAction?.Invoke();
            UpdateSizeAction = null;
        }

        public void CreateComponents(CanvasAnimatedControl canvas)
        {
            _inputComponent = new InputComponent(Window.Current, canvas);

            CanvasProperties.Width = canvas.ActualSize.X;
            CanvasProperties.Height = canvas.ActualSize.Y;
        }

        public void SetSize(Vector2 size)
        {
            UpdateSizeAction = new Action(() =>
            {
                CurrentTab?.SceneManager?.SetSize(size);
            });

            CanvasProperties.Width = size.X;
            CanvasProperties.Height = size.Y;
        }

        #endregion

        #region Update methods

        public void Update()
        {
            FrameStarted?.Invoke(this, EventArgs.Empty);

            if (_isIdle || CurrentTab == null || CurrentTab.SceneManager.IsIdle)
                return;

            ProcessInput();

            CurrentTab.SceneManager.Update();

#if DEBUG
            var collectCount = GC.CollectionCount(2);

            if (collectCount > _collectCount)
            {
                _collectCount = collectCount;
            }
#endif
        }

        private void ProcessInput()
        {
            _inputComponent.Update();

            while (_inputComponent.Keyboard.Queue.Count > 0)
            {
                var keyboardInput = _inputComponent.Keyboard.Queue.Dequeue();

                CurrentTab.SceneManager.ProcessKeyboardInput(keyboardInput);
            }

            while (_inputComponent.Mouse.Queue.Count > 0)
            {
                var mouseInput = _inputComponent.Mouse.Queue.Dequeue();

                CurrentTab.SceneManager.ProcessMouseInput(mouseInput);
            }
        }


        #endregion

        #region Draw methods

        public void Draw(CanvasDevice device, CanvasDrawingSession drawingSession)
        {
            if (_isIdle || CurrentTab == null || CurrentTab.SceneManager.IsIdle)
                return;

            using (var deviceLock = device.Lock())
            {
                CurrentTab.SceneManager.Draw(drawingSession);

#if DEBUG
                drawingSession.Transform = Matrix3x2.Identity;
                drawingSession.DrawText(_collectCount.ToString(), new Vector2(20, 20), Colors.Red);
#endif
            }
        }


        #endregion
        
        #endregion
    }
}