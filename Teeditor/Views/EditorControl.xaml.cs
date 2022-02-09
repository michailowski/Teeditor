using System;
using System.Numerics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Teeditor.ViewModels;

namespace Teeditor.Views
{
    public sealed partial class EditorControl : UserControl
    {
        internal EditorViewModel Source
        {
            get => (EditorViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(EditorViewModel),
                typeof(EditorControl), new PropertyMetadata(null, SourceProperty_Changed));

        private EditorViewModel _viewModel;

        public EditorControl()
        {
            this.InitializeComponent();
        }

        private void MainCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args) 
            => _viewModel?.CreateComponents(sender);

        private void MainCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args) 
            => _viewModel?.Update();

        private void MainCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) 
            => _viewModel?.Draw(sender.Device, args.DrawingSession);

        private static void SourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EditorControl)d;

            if (e.OldValue is EditorViewModel oldDataContext)
            {
                oldDataContext.ActionOnGameLoopRunned -= control.DataContext_ActionOnGameLoopRunned;
            }

            if (e.NewValue is EditorViewModel newDataContext)
            {
                control.DataContext = control._viewModel = newDataContext;

                newDataContext.ActionOnGameLoopRunned += control.DataContext_ActionOnGameLoopRunned;
            }
        }

        private void DataContext_ActionOnGameLoopRunned(object sender, Action action)
        {
            var ignoredAction = MainCanvas.RunOnGameLoopThreadAsync(new DispatchedHandler(action));
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e) 
            => _viewModel?.SetSize(e.NewSize.ToVector2());

        private void EditorControl_Unloaded(object sender, RoutedEventArgs args)
        {
            MainCanvas.RemoveFromVisualTree();
            MainCanvas = null;
        }
    }
}
