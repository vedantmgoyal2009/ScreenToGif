using ScreenToGif.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline
{
    /// <summary>
    /// Track that displays sequences for the timeline.
    /// </summary>
    public class TimelineTrack : Control
    {
        public static readonly DependencyProperty TrackNameProperty = DependencyProperty.Register(nameof(TrackName), typeof(string), typeof(TimelineTrack), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IsTrackVisibleProperty = DependencyProperty.Register(nameof(IsTrackVisible), typeof(bool), typeof(TimelineTrack), new PropertyMetadata(true));
        public static readonly DependencyProperty IsTrackLockedProperty = DependencyProperty.Register(nameof(IsTrackLocked), typeof(bool), typeof(TimelineTrack), new PropertyMetadata(false));
        public static readonly DependencyProperty TrackAccentProperty = DependencyProperty.Register(nameof(TrackAccent), typeof(Brush), typeof(TimelineTrack), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty SequencesProperty = DependencyProperty.Register(nameof(Sequences), typeof(ObservableCollection<ISequence>), typeof(TimelineTrack), new PropertyMetadata(null));
        
        public string TrackName
        {
            get => (string)GetValue(TrackNameProperty);
            set => SetValue(TrackNameProperty, value);
        }

        public bool IsTrackVisible
        {
            get => (bool)GetValue(IsTrackVisibleProperty);
            set => SetValue(IsTrackVisibleProperty, value);
        }

        public bool IsTrackLocked
        {
            get => (bool)GetValue(IsTrackLockedProperty);
            set => SetValue(IsTrackLockedProperty, value);
        }

        public Brush TrackAccent
        {
            get => (Brush)GetValue(TrackAccentProperty);
            set => SetValue(TrackAccentProperty, value);
        }

        public ObservableCollection<ISequence> Sequences
        {
            get => (ObservableCollection<ISequence>)GetValue(SequencesProperty);
            set => SetValue(SequencesProperty, value);
        }
        
        static TimelineTrack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineTrack), new FrameworkPropertyMetadata(typeof(TimelineTrack)));
        }

        //Header > content
        //Allow dragging sequences
        //Render based on ViewportStart, ViewPortEnd

        //Render Sequences as elements.
        //So, insert how many elements(sequences) as needed and position them based on Viewport and sequence positions.
    }
}