using ScreenToGif.Domain.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls.Timeline
{
    public class Timeline : Control
    {
        #region Properties

        public static readonly DependencyProperty TracksProperty = DependencyProperty.Register(nameof(Tracks), typeof(ObservableCollection<ITrack>), typeof(Timeline), new PropertyMetadata(null, Tracks_Changed));
        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero, Viewport_Changed, ViewportStart_Coerce));
        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero, Viewport_Changed));
        public static readonly DependencyPropertyKey ViewportSpanPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ViewportSpan), typeof(TimeSpan), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero, Current_Changed, Current_Coerce));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(Timeline), new PropertyMetadata((TimeSpan?)null));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(Timeline), new PropertyMetadata((TimeSpan?)null));
        
        public ObservableCollection<ITrack> Tracks
        {
            get => (ObservableCollection<ITrack>)GetValue(TracksProperty);
            set => SetValue(TracksProperty, value);
        }

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
        
        public TimeSpan ViewportSpan
        {
            get => (TimeSpan)GetValue(ViewportSpanPropertyKey.DependencyProperty);
            private set => SetValue(ViewportSpanPropertyKey, value);
        }
        
        public TimeSpan Current
        {
            get => (TimeSpan)GetValue(CurrentProperty);
            set => SetValue(CurrentProperty, value);
        }

        public TimeSpan? SelectionStart
        {
            get => (TimeSpan?)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public TimeSpan? SelectionEnd
        {
            get => (TimeSpan?)GetValue(SelectionEndProperty);
            set => SetValue(SelectionEndProperty, value);
        }

        #endregion

        static Timeline()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));
        }

        public Timeline()
        {
            SetCurrentValue(TracksProperty, new ObservableCollection<ITrack>());
        }

        //Maximum time, but allowing users to move tracks further.
        //The view port will need to virtualized horizontally, based on the zoom level, size of the control and center of view, the renderization will happen.

        //Multiple tracks.
        //Fixed track tab (left side), with lock and visibility options.
        //Layer height.
        //Vertical scroll for layers.

        private static void Tracks_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Timeline timeline)
                return;

            if (timeline.ViewportEnd == TimeSpan.Zero)
                timeline.ViewportEnd = timeline.Tracks?.Any() == true ? timeline.Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max() : TimeSpan.Zero;
        }
        
        private static void Current_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static object Current_Coerce(DependencyObject d, object baseValue)
        {
            if (baseValue is TimeSpan value)
                return value.TotalMilliseconds < 0 ? TimeSpan.Zero : value;

            return baseValue;
        }

        private static void Viewport_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Timeline timeline)
                return;

            timeline.ViewportSpan = timeline.ViewportEnd - timeline.ViewportStart;
        }

        private static object ViewportStart_Coerce(DependencyObject d, object baseValue)
        {
            if (baseValue is TimeSpan value)
                return value.TotalMilliseconds < 0 ? TimeSpan.Zero : value;

            return baseValue;
        }
    }
}