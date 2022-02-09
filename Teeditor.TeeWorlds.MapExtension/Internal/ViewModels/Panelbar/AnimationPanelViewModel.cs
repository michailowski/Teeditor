using Microsoft.Graphics.Canvas;
using System;
using System.Linq;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Teeditor.Common.Models.Tab;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Panelbar
{

    internal class AnimationPanelViewModel : PanelViewModelBase
    {
        private SceneTimer _timer;

        private TimelineDrawer _timelineDrawer;
        private EnvelopesDrawer _envelopesDrawer;

        private EnvelopesContainer _envelopesContainer;
        private GroupedLayersContainer _groupedLayersContainer;
        private SidebarManager _sidebarManager;

        private double _contentWidth;
        private double _contentHeight;

        private MapEnvelopeFactory _envelopeFactory;
        private MapEnvelopePointColorFactory _pointColorFactory;
        private MapEnvelopePointPositionFactory _pointPositionFactory;

        public EnvelopesContainer EnvelopesContainer
        {
            get => _envelopesContainer;
            set => Set(ref _envelopesContainer, value);
        }

        public int SecondLengthInPixels { get; private set; } = 100;

        public double ContentWidth
        {
            get => _contentWidth;
            set => Set(ref _contentWidth, value);
        }

        public double ContentHeight
        {
            get => _contentHeight;
            set => Set(ref _contentHeight, value);
        }

        public Keyframe HoveredKeyframe => _envelopesDrawer.HoveredKeyframe;

        public EnvelopeSegment HoveredSegment => _envelopesDrawer.HoveredSegment;

        public TimeSpan Time
        {
            get => _timer.Time;
            set => _timer.Time = value;
        }

        public bool IsStarted
        {
            get => _timer.IsStarted;
            set => _timer.IsStarted = value;
        }

        public event EventHandler ForceContentRegionsInvalidation;

        public AnimationPanelViewModel()
        {
            Label = "Animation";
            MenuText = "Animation Panel";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };

            _envelopeFactory = new MapEnvelopeFactory();
            _pointColorFactory = new MapEnvelopePointColorFactory();
            _pointPositionFactory = new MapEnvelopePointPositionFactory();

            _timelineDrawer = new TimelineDrawer(SecondLengthInPixels);
            _envelopesDrawer = new EnvelopesDrawer(SecondLengthInPixels);

            _envelopesDrawer.ContentBoundsChanged += EnvelopesDrawer_ContentBoundsChanged;
            _envelopesDrawer.ForceRegionsInvalidation += EnvelopesDrawer_ForceRegionsInvalidation;
        }

        public override void SetTab(ITab tab)
        {
            var map = tab?.Data as Map;

            EnvelopesContainer = map.EnvelopesContainer;
            _groupedLayersContainer = map.GroupedLayersContainer;

            _envelopesDrawer.SetEnvelopes(EnvelopesContainer);

            var sceneManager = tab?.SceneManager as MapSceneManager;
            _timer = sceneManager?.Timer;

            _sidebarManager = tab?.SidebarManager as SidebarManager;

            DynamicModel = _timer;
        }

        #region Draw

        public void DrawTimeline(CanvasDrawingSession ds, Rect region)
            => _timelineDrawer.Draw(ds, region);

        public void DrawEnvelopes(CanvasDrawingSession ds, Rect region)
            => _envelopesDrawer.Draw(ds, region);

        #endregion

        #region Mouse Move and Dragging

        public void SetContentPointerPosition(Point pointerPosition)
            => _envelopesDrawer.SetPointerPosition(pointerPosition);
        
        public bool TryContentDragging(Point pointerPosition)
            => _envelopesDrawer.TryDragging(pointerPosition);

        public bool TryStartContentDragging(Point pointerPosition)
            => _envelopesDrawer.TryStartDragging(pointerPosition);

        public bool TryContentMoveByOffset(int offset)
            => _envelopesDrawer.TryMoveByOffset(offset);
        
        public void EndContentDragging()
            => _envelopesDrawer.EndDragging();

        public void ResetPointerPosition()
            => _envelopesDrawer.ResetPointerPosition();

        public bool TryStartContentDragBoosting(KeyframeBoostingDirection boostingDirection)
            => _envelopesDrawer.TryActivateDragBoosting(boostingDirection);

        public void EndContentDragBoosting()
            => _envelopesDrawer.DeactivateDragBoosting();

        #endregion

        #region Get and Set Methods

        public Rect GetInvalidatedRegions(Rect canBeInvalidate)
            => _envelopesDrawer.GetInvalidatedRegion(canBeInvalidate);

        public bool TryIncreaseTimelineInterval()
        {
            if (SecondLengthInPixels >= 1000)
                return false;

            SecondLengthInPixels += 100;

            _timelineDrawer.SetSecondLength(SecondLengthInPixels);
            _envelopesDrawer.SetSecondLength(SecondLengthInPixels);

            OnPropertyChanged("Time");

            return true;
        }

        public bool TryDecreaseTimelineInterval()
        {
            if (SecondLengthInPixels <= 100)
                return false;

            SecondLengthInPixels -= 100;

            _timelineDrawer.SetSecondLength(SecondLengthInPixels);
            _envelopesDrawer.SetSecondLength(SecondLengthInPixels);

            OnPropertyChanged("Time");

            return true;
        }

        public void ResetTimer() => _timer.Reset();

        public Thickness GetMarginByTime(TimeSpan time)
            => new Thickness(GetPixelsByTime(time), 0, 0, 0);

        public double GetPixelsByTime(TimeSpan time)
            => time.TotalSeconds * SecondLengthInPixels;

        public bool TrySetTimeByPixels(double pixels)
        {
            if (IsStarted)
                return false;

            pixels = Math.Max(pixels, 0);

            Time = TimeSpan.FromSeconds(pixels / SecondLengthInPixels);

            return true;
        }

        public int GetEnvelopeIndex(MapEnvelope envelope)
            => _envelopesContainer.Items.IndexOf(envelope);

        #endregion

        #region Logic

        private void EnvelopesDrawer_ForceRegionsInvalidation(object sender, EventArgs e)
            => ForceContentRegionsInvalidation?.Invoke(sender, EventArgs.Empty);

        private void EnvelopesDrawer_ContentBoundsChanged(object sender, EventArgs e)
        {
            var contentRect = _envelopesDrawer.GetContentBounds();

            ContentWidth = contentRect.Width;
            ContentHeight = contentRect.Height;
        }

        public void AddColorEnvelope()
        {
            _envelopeFactory.SetType(EnvelopeType.Color);
            var newEnvelope = (MapEnvelope)_envelopeFactory.Create();
            EnvelopesContainer.Add(newEnvelope);
        }

        public void AddPositionEnvelope()
        {
            _envelopeFactory.SetType(EnvelopeType.Position);
            var newEnvelope = (MapEnvelope)_envelopeFactory.Create();
            EnvelopesContainer.Add(newEnvelope);
        }

        public void OpenProperties(MapEnvelope envelope)
        {
            _sidebarManager.PropertyBoxItem = envelope;
            _sidebarManager?.TryOpen(typeof(PropertiesBoxControl));
        }

        public void RemoveEnvelopeWithBindings(MapEnvelope envelope)
        {
            var envelopeIndex = EnvelopesContainer.Items.IndexOf(envelope);
            var groups = _groupedLayersContainer.Groups;

            foreach (var group in groups)
            {
                foreach (var layer in group.Layers)
                {
                    if (layer is MapQuadsLayer quadsLayer)
                    {
                        foreach (var quad in quadsLayer.Quads)
                        {
                            if (quad.ColorEnvIndex == envelopeIndex)
                            {
                                quad.ColorEnvIndex = -1;
                            }
                            else if (quad.PosEnvIndex == envelopeIndex)
                            {
                                quad.PosEnvIndex = -1;
                            }
                        }
                    }
                    else if (layer is MapTilesLayer tilesLayer)
                    {
                        if (tilesLayer.ColorEnvelope == envelope)
                        {
                            tilesLayer.ColorEnvelope = null;
                        }
                    }
                }
            }

            EnvelopesContainer.Remove(envelope);
        }

        public void AddKeyframeAtTimeCursor(MapEnvelope envelope)
        {
            MapEnvelopePoint point;

            if (envelope.Type == EnvelopeType.Color)
            {
                _pointColorFactory.SetTime(Time);
                point = (MapEnvelopePoint)_pointColorFactory.Create();
            }
            else if (envelope.Type == EnvelopeType.Position)
            {
                _pointPositionFactory.SetTime(Time);
                point = (MapEnvelopePoint)_pointPositionFactory.Create();
            }
            else
            {
                return;
            }

            var sortedDescPoints = envelope.Points.OrderByDescending(x => x.Time);
            var leftEnvelope = sortedDescPoints.FirstOrDefault(x => x.Time <= Time);

            var sortedPoints = envelope.Points.OrderBy(x => x.Time);
            var rightEnvelope = sortedPoints.FirstOrDefault(x => x.Time >= Time);

            if (rightEnvelope == null)
            {
                envelope.Points.Add(point);
            }
            else
            {
                var index = leftEnvelope != null ? envelope.Points.IndexOf(leftEnvelope) + 1 : 0;

                envelope.Points.Insert(index, point);
            }
        }

        public void RemoveEnvelopePoint(MapEnvelopePoint point)
            => HoveredKeyframe.Envelope.Points.Remove(point);

        #endregion
    }
}
