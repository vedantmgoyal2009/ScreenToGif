using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenToGif.Controls.Timeline
{
    public class ViewportTrack : Track
    {
        #region Properties

        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(ViewportTrack),
            new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(ViewportTrack),
            new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(ViewportTrack),
            new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.AffectsArrange));

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

        #endregion
        
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Current.TotalMilliseconds >= ViewportEnd.TotalMilliseconds ||
                Current.TotalMilliseconds <= ViewportStart.TotalMilliseconds)
            {
                Visibility = Visibility.Hidden;
                return arrangeSize;
            }

            var min = ViewportStart.TotalMilliseconds;
            var range = Math.Max(0.0, ViewportEnd.TotalMilliseconds - min);
            var offset = Math.Min(range, Current.TotalMilliseconds - min);

            if (Visibility != Visibility.Visible)
                Visibility = Visibility.Visible;

            var leftMarging = Math.Max(0, Math.Min(arrangeSize.Width * (offset / range), arrangeSize.Width));

            if (leftMarging < 1)
                return arrangeSize;

            //Layout the pieces of track
            var pos = new Point { X = leftMarging - Thumb.DesiredSize.Width / 2d };

            var pieceSize = arrangeSize;
            pieceSize.Width = Thumb.DesiredSize.Width;

            Thumb?.Arrange(new Rect(pos, pieceSize));

            return arrangeSize;
        }
    }
}