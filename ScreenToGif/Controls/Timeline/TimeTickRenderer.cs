using System;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline
{
    public class TimeTickRenderer : FrameworkElement
    {
        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register(nameof(TickBrush), typeof(Brush), typeof(TimeTickRenderer), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(TimeTickRenderer), new PropertyMetadata(Brushes.Black));
        
        public TimeSpan ViewportEnd
        {
            get => (TimeSpan)GetValue(ViewportEndProperty);
            set => SetValue(ViewportEndProperty, value);
        }

        public TimeSpan ViewportStart
        {
            get => (TimeSpan)GetValue(ViewportStartProperty);
            set => SetValue(ViewportStartProperty, value);
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

        public Brush TickBrush
        {
            get => (Brush)GetValue(TickBrushProperty);
            set => SetValue(TickBrushProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen(TickBrush, 1);

            //Zoom based on mouse position (Adjust time).

            var Zoom = 1;

            //Maybe instead of defaulting to 30s, I'll use a timeSpan that equates to 0% zoom on the current display width.
            var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;
            var perPixel = inView / ActualWidth;

            //Based on viewSpan, decide when to show each tick. Should be dynamic.

            // < 200ms in view:
            //Large tick: Every 1000ms
            //Medium tick: Every 500ms
            //Small tick: Every 100ms
            //Very small tick: Every 10ms
            //Timestamp: Every 500ms

            //< 15000ms in view:
            //Large tick: Every 10.000ms
            //Medium tick: Every 5.000ms
            //Small tick: Every 1.000ms
            //Very small tick: Every 100ms
            //Timestamp: Every 5.000ms

            //Large
            var largeTickFrequency = 1000 / perPixel;
            var offset = ViewportStart.TotalMilliseconds % 1000;
            var offsetSize = offset / perPixel;

            Console.WriteLine($"{Zoom}% • {inView}ms = {ViewportStart} ~ {offset} = {offsetSize}px");

            for (var x = offsetSize; x < ActualWidth; x += largeTickFrequency)
                drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 16));

            if (perPixel > 9)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Medium, printed for each 500ms.
            var mediumTickFrequency = 500 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 500;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += mediumTickFrequency)
                drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 12));

            if (perPixel > 5)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Small, printed for each 100ms
            var smallTickFrequency = 100 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 100;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += smallTickFrequency)
                drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 8));

            if (perPixel > 1.5)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Very small, printed for each 10ms.
            var verySmallTickFrequency = 10 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 10;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += verySmallTickFrequency)
                drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 4));

            base.OnRender(drawingContext);
        }
    }
}