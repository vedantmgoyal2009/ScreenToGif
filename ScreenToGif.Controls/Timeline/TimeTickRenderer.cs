using ScreenToGif.Util.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline
{
    public class TimeTickRenderer : FrameworkElement
    {
        private Pen _tickPen;

        #region Properties

        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register(nameof(TickBrush), typeof(Brush), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, TickBrush_PropertyChanged));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

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

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            _tickPen = new Pen(TickBrush, 1);

            base.OnInitialized(e);
        }

        //Viewport span:
        //1 day:    24x60x60x1000  86.400.000ms
        //1h/24h:   60x60x1000     3.600.000ms
        //1m/60m:   60x1000        60.000ms
        //1s/60s:                  1000ms

        //Time,   TotalWidth, Tall/Short, Tall/Short
        //3:39.3, 3250px,     5s/1s,       73px/14px   219.000ms/3250px = 67.476ms/px
        //1:27.8, 3250px,     5s/1s,      184px/36px    87.800ms/3250px = 27.015ms/px
        //1:27.7, 3250px,     2s/1s,       73px/36px    87.700ms/3250px = 26.984ms/px
        //0:43.9, 3250px,     2s/1s,      147px/73px    43.900ms/3250px = 13.507ms/px
        //0:43.8, 3250px,     1s/.5s,      73px/36px    43.800ms/3250px = 13.476ms/px
        //0:22.0, 3250px,     1s/.5s,     147px/73px    22.000ms/3250px =  6.769ms/px
        //0:21.9, 3250px,     .5s/.1s,     73px/14px    21.900ms/3250px =  6.738ms/px
        //0:08.8, 3250px,     .5s/.1s,    184px/36px     8.800ms/3250px =  2.707ms/px
        //0:08.7, 3250px,     .2s/.1s,    184px/36px     8.700ms/3250px =  2.676ms/px
        //0:04.4, 3250px,     .2s/.1s,    147px/73px     4.400ms/3250px =  1.353ms/px
        //0:04.3, 3250px,     .1s/.05s,    74px/37px     4.300ms/3250px =  1.323ms/px
        //0:02.1, 3250px,     .1s/.05s,   154px/77px     2.100ms/3250px =  0.646ms/px
        //0:02.0, 3250px,     .05s/.01s,   80px/15px     2.000ms/3250px =  0.615ms/px

        //3:00.0, 3250px,     5s/1s,       89px/17px    180.000ms/3250px = 55.38ms/px
        //3:00.0, 2667px,     5s/1s,       73px/14px    180.000ms/2667px = 67.49ms/px
        //3:00.0, 2666px,     10s/5s,      147px/73px   180.000ms/2666px = 67.51ms/px
        //3:00.0, 1173px,     10s/5s,      64px/32px    180.000ms/1173px = 153.45ms/px
        //3:00.0, 1172px,     20s/10s,     129px/65px   180.000ms/1172px = 153.58ms/px
        //3:00.0, 588px,      20s/10s,     64px/32px    180.000ms/588px =  306.12ms/px
        //3:00.0, 587px,      30s/15s,     97px/48px    180.000ms/587px =  306.64ms/px
        //3:00.0, 393px,      30s/15s,     64px/32px    180.000ms/393px =  458.01ms/px
        //3:00.0, 392px,      1m/30s,      130px/64px   180.000ms/392px =  459.18ms/px
        //3:00.0, 198px,      1m/30s,      198px/32px   180.000ms/198px =  909.09ms/px
        //3:00.0, 197px,      2m/1m,       197px/64px   180.000ms/197px =  913.70ms/px

        protected override void OnRender(DrawingContext drawingContext)
        {
            //How many milliseconds are in view.
            var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;

            if (inView < 1)
            {
                base.OnRender(drawingContext);
                return;
            }

            //How many milliseconds each pixel represents.
            var perPixel = inView / ActualWidth;

            DrawTicks(drawingContext, inView, perPixel);

            ////m_scale is the pixels per frame ratio
            //var mScale = (ActualWidth - 2) / -1;

            //if (mScale == 0)
            //    mScale = -1;

            //var mFps = 60;
            //var mTimecodeWidth = 40;
            //var mSecondsPerTick = (mTimecodeWidth * 1.8) / mScale / mFps;

            //if (mSecondsPerTick > 3600)
            //    mSecondsPerTick += 3600 - mSecondsPerTick % 3600; // force to a multiple of one hour
            //else if (mSecondsPerTick > 300)
            //    mSecondsPerTick += 300 - mSecondsPerTick % 300; // force to a multiple of 5 minutes
            //else if (mSecondsPerTick > 60)
            //    mSecondsPerTick += 60 - mSecondsPerTick % 60; // force to a multiple of one minute
            //else if (mSecondsPerTick > 5)
            //    mSecondsPerTick += 10 - mSecondsPerTick % 10; // force to a multiple of 10 seconds
            //else if (mSecondsPerTick > 2)
            //    mSecondsPerTick += 5 - mSecondsPerTick % 5; //Force to a multiple of 5 seconds

            ////m_interval is the number of pixels per major tick to be labeled with time
            //var mInterval = Math.Round(mSecondsPerTick * mFps * mScale);
            //var lInterval = mInterval;

            // draw time ticks
            //if (l_interval > 2)
            //{
            //    for (var x = 0d; x < ActualWidth; x += l_interval)
            //    {
            //        drawingContext.DrawLine(_tickPen, new Point(x, ActualHeight), new Point(x, ActualHeight - 1));

            //        //p.drawLine(x, l_selectionSize, x, l_height - 1);

            //        if (x + l_interval / 4 < ActualWidth)
            //            drawingContext.DrawLine(_tickPen, new Point(x + l_interval / 4, ActualHeight - 3), new Point(x + l_interval / 4, ActualHeight - 1));

            //        if (x + l_interval / 2 < ActualWidth)
            //            drawingContext.DrawLine(_tickPen, new Point(x + l_interval / 2, ActualHeight - 7), new Point(x + l_interval / 2, ActualHeight - 1));

            //        if (x + l_interval * 3 / 4 < ActualWidth)
            //            drawingContext.DrawLine(_tickPen, new Point(x + l_interval * 3 / 4, ActualHeight - 3), new Point(x + l_interval * 3 / 4, ActualHeight - 1));
            //    }
            //}
            
            base.OnRender(drawingContext);
        }

        private void DrawTicks(DrawingContext drawingContext, double inView, double perPixel)
        {
            double distanceShort;
            double distanceTall;

            //TODO: Improve this, find a proper calculation for any kind of timestamp.
            switch (perPixel)
            {
                case < 0.646:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 10, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 50, null) / 100d;
                    break;
                case < 1.323:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 50, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 100, null) / 100d;
                    break;
                case < 2.672:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 100, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 200, null) / 100d;
                    break;
                case < 6.738:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 100, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 500, null) / 100d;
                    break;
                case < 13.476:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 500, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 1000, null) / 100d;
                    break;
                case < 26.984:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 1000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 2000, null) / 100d;
                    break;
                case < 67.49:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 2000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 5000, null) / 100d;
                    break;
                case < 153.45:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 5000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 10000, null) / 100d;
                    break;
                case < 306.12:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 10000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 20000, null) / 100d;
                    break;
                case < 458.01:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 15000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 30000, null) / 100d;
                    break;
                case < 909.09:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 30000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 60000, null) / 100d;
                    break;
                default:
                    distanceShort = ActualWidth * MathExtensions.CrossMultiplication(inView, 60000, null) / 100d;
                    distanceTall = ActualWidth * MathExtensions.CrossMultiplication(inView, 120000, null) / 100d;
                    break;
            }

            //TODO: Is this offset right?
            var startShortOffset = (MathExtensions.CrossMultiplication(inView, null, ViewportStart.TotalMilliseconds) / perPixel) % distanceShort;
            var startTallOffset = (MathExtensions.CrossMultiplication(inView, null, ViewportStart.TotalMilliseconds) / perPixel) % distanceTall;

            for (var x = startShortOffset; x <= ActualWidth; x += distanceShort)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 4));

            for (var x = startTallOffset; x <= ActualWidth; x += distanceTall)
            {
                //Draw tick.
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 8));

                //Draw timestamp text.
                var timestamp = ViewportStart.Add(TimeSpan.FromMilliseconds(x * perPixel));

                //var text = timestamp.ToString(timestamp.TotalMilliseconds < 1000 ? @"m\:ss,f" : timestamp.TotalMinutes < 1 ? @"s\,f's'" : timestamp.TotalHours < 1 ? @"mm\:ss'm'" : @"HH\:mm'h'");
                var text = timestamp.ToString(timestamp.TotalMilliseconds < 100 ? @"m\:ss\,fff" : timestamp.TotalMinutes < 1 ? @"m\:ss\,f" : timestamp.TotalHours < 1 ? @"mm\:ss" : @"HH\:mm");

                var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10d, TickBrush, 96d);

                //if (x + formattedText.MinWidth / 2 > ActualWidth || x - formattedText.MinWidth / 2 < 0)
                if (x - formattedText.MinWidth / 2 < 0)
                    continue;

                drawingContext.DrawText(formattedText, new Point(x - formattedText.MinWidth / 2, 10));
            }
        }

        private static void TickBrush_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimeTickRenderer rendered)
                return;

            rendered._tickPen = new Pen(rendered.TickBrush, 1);
        }
    }
}