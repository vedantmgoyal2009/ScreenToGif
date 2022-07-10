using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScreenToGif.Util.Extensions;

namespace ScreenToGif.Controls.Timeline
{
    [TemplatePart(Name = TimeTickRendererId, Type = typeof(TimeTickRenderer))]
    [TemplatePart(Name = ViewportTrackId, Type = typeof(ViewportTrack))]
    public class TimeBar : Control
    {
        private const string TimeTickRendererId = "TimeTickRenderer";
        private const string ViewportTrackId = "ViewportTrack";

        /// <summary>
        /// The threshold of mouse wheel delta to enable the wheel event.
        /// </summary>
        private const double MouseWheelSelectionChangeThreshold = 100;

        /// <summary>
        /// The aggregate of mouse wheel delta since the last mouse wheel event.
        /// </summary>
        private double _mouseWheelCumulativeDelta;

        private TimeTickRenderer _rendered;
        private ViewportTrack _viewportTrack;

        #region Properties

        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(TimeBar), new FrameworkPropertyMetadata(TimeSpan.Zero, Current_Changed, Current_Coerce));
        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(TimeBar), new FrameworkPropertyMetadata(TimeSpan.Zero));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(TimeBar), new PropertyMetadata(default(TimeSpan)));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(TimeBar), new PropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(TimeBar), new PropertyMetadata(default(TimeSpan?)));
        
        public TimeSpan ViewportStart
        {
            get => (TimeSpan)GetValue(ViewportStartProperty);
            set => SetValue(ViewportStartProperty, value);
        }

        public TimeSpan ViewportEnd
        {
            get => (TimeSpan)GetValue(ViewportEndProperty);
            set => SetValue(ViewportEndProperty, value);
        }

        public TimeSpan Current
        {
            get => (TimeSpan)GetValue(CurrentProperty);
            set => SetValue(CurrentProperty, value);
        }

        public TimeSpan? SelectionStart
        {
            get => (TimeSpan)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public TimeSpan? SelectionEnd
        {
            get => (TimeSpan?)GetValue(SelectionEndProperty);
            set => SetValue(SelectionEndProperty, value);
        }

        #endregion

        static TimeBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeBar), new FrameworkPropertyMetadata(typeof(TimeBar)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rendered = GetTemplateChild(TimeTickRendererId) as TimeTickRenderer;
            _viewportTrack = GetTemplateChild(ViewportTrackId) as ViewportTrack;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            #region Mouse wheel debounce

            _mouseWheelCumulativeDelta += e.Delta;

            if (!Math.Abs(_mouseWheelCumulativeDelta).GreaterThan(MouseWheelSelectionChangeThreshold))
            {
                base.OnMouseWheel(e);
                return;
            }

            e.Handled = true;
            base.OnMouseWheel(e);

            _mouseWheelCumulativeDelta = 0;

            #endregion

            //Zoom in/out based on ViewportStart and mouse position.

            var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;
            var perPixel = inView / ActualWidth;

            ViewportStart += TimeSpan.FromMilliseconds((e.Delta > 0 ? 1 : -1) * perPixel);
            ViewportEnd += TimeSpan.FromMilliseconds((e.Delta > 0 ? 1 : -1) * perPixel);
        }

        //Visual indicator of Current needs to be created.
        //Visual indicator of SelectionStart/End needs to be created.
        //These elements need to be draggable, resizable.

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                //TODO: Shift + Click and drag to select
            }
            else
                MoveToPoint(e.GetPosition(this));
            
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            //Right Click Down + Drag + Right Click Up to zoom to that selection (capture cursor).
            //Same as HorizontalScrollBar
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                MoveToPoint(e.GetPosition(this));

            base.OnMouseMove(e);
        }

        //OnMouseLeftButtonUp
        //OnMouseRightButtonUp

        private static void Current_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static object Current_Coerce(DependencyObject d, object baseValue)
        {
            if (baseValue is TimeSpan value)
                return value.TotalMilliseconds < 0 ? TimeSpan.Zero : value;

            return baseValue;
        }

        private void MoveToPoint(Point position)
        {
            var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;

            if (inView < 1)
                return;
            
            //How many milliseconds each pixel represents.
            var perPixel = inView / ActualWidth;

            Current = ViewportStart + TimeSpan.FromMilliseconds(position.X * perPixel);
        }
    }
}