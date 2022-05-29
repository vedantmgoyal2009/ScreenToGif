using ScreenToGif.Util.Extensions;
using ScreenToGif.ViewModel.Project;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline;

[TemplatePart(Name = ResizableThumbId, Type = typeof(ResizableThumb))]
[TemplatePart(Name = SimplifiedTrackId, Type = typeof(SimplifiedTrack))]
public class HorizontalTimelineScrollBar : RangeBase
{
    private const string ResizableThumbId = "ResizableThumb";
    private const string SimplifiedTrackId = "SimplifiedTrack";

    private ResizableThumb _resizableThumb;
    private SimplifiedTrack _simplifiedTrack;
    private bool _skip;
    private Vector _thumbOffset;
    private Point _latestRightButtonClickPoint = new(-1, -1);

    #region Properties

    public static readonly DependencyProperty ViewportSizeProperty = DependencyProperty.Register(nameof(ViewportSize), typeof(double), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(double), ViewportSize_PropertyChanged));
    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(nameof(EndValue), typeof(double), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(double), ViewportSize_PropertyChanged));

    public static readonly DependencyProperty StartProperty = DependencyProperty.Register(nameof(Start), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan), Position_PropertyChanged));
    public static readonly DependencyProperty EndProperty = DependencyProperty.Register(nameof(End), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan), Position_PropertyChanged));
    public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan)));
    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    public static readonly DependencyProperty TracksProperty = DependencyProperty.Register(nameof(Tracks), typeof(ObservableCollection<TrackViewModel>), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(null, Tracks_PropertyChanged));

    public double ViewportSize
    {
        get => (double)GetValue(ViewportSizeProperty);
        set => SetValue(ViewportSizeProperty, value);
    }

    public double EndValue
    {
        get => (double)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }
    
    public TimeSpan Start
    {
        get => (TimeSpan)GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }
    
    public TimeSpan End
    {
        get => (TimeSpan)GetValue(EndProperty);
        set => SetValue(EndProperty, value);
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
    
    /// <summary>
    /// The current tracks of the project.
    /// The tracks and its sequences are used to create a mini-preview to be displayed as the background for the scrollbar.
    /// </summary>
    public ObservableCollection<TrackViewModel> Tracks
    {
        get => (ObservableCollection<TrackViewModel>)GetValue(TracksProperty);
        set => SetValue(TracksProperty, value);
    }

    #endregion

    static HorizontalTimelineScrollBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(typeof(HorizontalTimelineScrollBar)));
    }

    public HorizontalTimelineScrollBar()
    {
        //I cannot have a default value as the metadata for the dependency property.
        //https://stackoverflow.com/a/39046732/1735672
        SetCurrentValue(TracksProperty, new ObservableCollection<TrackViewModel>());
        SetCurrentValue(MaximumProperty, 100d);
        SetCurrentValue(MinimumProperty, 0d);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        _resizableThumb = GetTemplateChild(ResizableThumbId) as ResizableThumb;
        _simplifiedTrack = GetTemplateChild(SimplifiedTrackId) as SimplifiedTrack;

        if (_resizableThumb != null)
        {
            _resizableThumb.LeftDragDelta += LeftThumb_DragDelta;
            _resizableThumb.RightDragDelta += RightThumb_DragDelta;
        }

        if (Tracks != null)
            Tracks.CollectionChanged += (_, _) => RenderBackground();

        ValueChanged += (_, _) => InterpretPosition();

        MaximumProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(ViewportSize_PropertyChanged));
        MinimumProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(ViewportSize_PropertyChanged));
        
        EventManager.RegisterClassHandler(typeof(HorizontalTimelineScrollBar), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));

        RenderBackground();
    }

    /// <summary>
    /// ScrollBar supports 'Move-To-Point' by pre-processes Shift+MouseLeftButton Click.
    /// </summary>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        //Move selection to new position, but only if click happened in the track and not in the Thumb.
        if (_simplifiedTrack.Thumb is { IsMouseOver: true })
            return;

        _thumbOffset = new Vector();

        //Move Thumb to the Mouse location.
        var position = e.MouseDevice.GetPosition(_simplifiedTrack);
        var newValue = _simplifiedTrack.ValueFromPoint(position);

        Value = newValue;

        if (_simplifiedTrack.Thumb is { IsMouseOver: true })
            _thumbOffset = e.MouseDevice.GetPosition(_simplifiedTrack.Thumb) - new Point(_simplifiedTrack.Thumb.ActualWidth * 0.5, _simplifiedTrack.Thumb.ActualHeight * 0.5);
        else
            e.Handled = true;

        base.OnPreviewMouseLeftButtonDown(e);
    }

    /// <summary>
    /// ScrollBar need to remember the point which ContextMenu is invoke in order to perform 'Scroll Here' command correctly.
    /// </summary>
    protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
    {
        //Remember the mouse point relative to the SimplifiedTrack, or clear it.
        _latestRightButtonClickPoint = _simplifiedTrack != null ? e.MouseDevice.GetPosition(_simplifiedTrack) : new Point(-1, -1);

        base.OnPreviewMouseRightButtonUp(e);
    }

    private static void ViewportSize_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HorizontalTimelineScrollBar scrollBar)
            return;

        //When ViewportSize changes, the End property will change.
        scrollBar.InterpretPosition();
    }

    private static void Position_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HorizontalTimelineScrollBar scrollBar)
            return;

        //When Start/End changes, the Thumb bar needs to be recalculated.
        scrollBar.CalculateScroll();
    }

    private static void Tracks_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HorizontalTimelineScrollBar scrollBar)
            return;

        scrollBar.RenderBackground();
        scrollBar.CalculateScroll();
    }

    private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is not HorizontalTimelineScrollBar scrollBar)
            return;

        scrollBar.UpdateValue(e.HorizontalChange + scrollBar._thumbOffset.X, e.VerticalChange + scrollBar._thumbOffset.Y);
    }

    private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        e.Handled = true;

        var nextViewport = ViewportSize - e.HorizontalChange;
        var nextValue = Value + e.HorizontalChange;

        //ViewportSize = Math.Max(Math.Min(nextViewport, Maximum - nextValue), 20);
        //Value = Math.Max(Math.Min(nextValue, Maximum - ViewportSize), 0);
        Value = Math.Max(Math.Min(nextValue, EndValue), 0);
        
        System.Diagnostics.Debug.WriteLine($"Value: {Value} EndValue: '{EndValue}' Maximum: {Maximum} {((EndValue).NearlyEquals(Maximum) ? "(Equals)" : "")}");
    }

    private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        e.Handled = true;

        //var next = ViewportSize + e.HorizontalChange;
        var next = EndValue + e.HorizontalChange;

        //ViewportSize = Math.Max(Math.Min(next, Maximum - Value), 20);
        EndValue = Math.Max(Math.Min(next, Maximum), 20);

        System.Diagnostics.Debug.WriteLine($"Value: {Value} EndValue: '{EndValue}' Maximum: {Maximum} {((EndValue).NearlyEquals(Maximum) ? "(Equals)" : "")}");
    }

    private void RenderBackground()
    {
        if (Tracks == null || Tracks.Count == 0)
            return;

        var drawing = new DrawingGroup();

        //Draw base background, like black or white.
        //If Horizontal, draw the sequences of each track in the correct position.
        //If Vertical, draw the tracks as they ocupy the height of the viewport.

        var maximum = Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;
        var verticalStep = 5; //maximum / Tracks.Count / 2; //Thickness is not so great.

        foreach (var track in Tracks)
        {
            foreach (var sequence in track.Sequences)
            {
                drawing.Children.Add(new GeometryDrawing
                {
                    Brush = track.Accent,
                    Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, sequence.StartTime.TotalMilliseconds, null), verticalStep * track.Id, MathExtensions.CrossMultiplication(maximum, (sequence.EndTime - sequence.StartTime).TotalMilliseconds, null), verticalStep / 3))
                });
            }
        }

        Background = new DrawingBrush
        {
            Stretch = Stretch.Fill,
            Viewport = new Rect(0, 0, 1, 1),
            ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
            Viewbox = new Rect(0, 0, 1, 1),
            ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
            Drawing = drawing
        };
    }

    private void CalculateScroll()
    {
        if (_skip || Tracks == null || Tracks.Count == 0)
            return;

        _skip = true;

        Minimum = 0;
        Maximum = Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;
        //ViewportSize = (End - Start).TotalMilliseconds;
        Value = Start.TotalMilliseconds;
        EndValue = End.TotalMilliseconds;

        _skip = false;

        //var size = MathExtensions.CrossMultiplication(maximum, (End - Start).TotalMilliseconds, null);
        //var current = MathExtensions.CrossMultiplication(maximum, Start.TotalMilliseconds, null);

        //Maximum = 100;
        //Minimum = 0;
        //ViewportSize = size;
        //Value = current;
    }

    private void InterpretPosition()
    {
        if (_skip)
            return;

        _skip = true;

        Start = TimeSpan.FromMilliseconds(Value);
        //End = TimeSpan.FromMilliseconds(Value + ViewportSize);
        End = TimeSpan.FromMilliseconds(EndValue);

        _skip = false;

        //var maximum = Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;
        //Start = TimeSpan.FromMilliseconds(maximum / (MathExtensions.CrossMultiplication(Maximum, null, Value) / 100));
        //End = TimeSpan.FromMilliseconds(maximum / (MathExtensions.CrossMultiplication(Maximum, null, Value + ViewportSize) / 100));
    }

    /// <summary>
    /// Update ScrollBar Value based on the Thumb drag delta.
    /// </summary>
    private void UpdateValue(double horizontalDragDelta, double verticalDragDelta)
    {
        if (_simplifiedTrack == null)
            return;

        var valueDelta = _simplifiedTrack.ValueFromDistance(horizontalDragDelta, verticalDragDelta);

        if (double.IsInfinity(valueDelta) || valueDelta.NearlyEquals(0d))
            return;

        var currentValue = Value;
        var currentEndValue = EndValue;

        //var newValue = Math.Max(Math.Min(Maximum - ViewportSize, currentValue + valueDelta), 0);
        var newValue = Math.Max(Math.Min(Maximum - (EndValue - Value), currentValue + valueDelta), 0);
        var newEndValue = Math.Max(Math.Min(Maximum, currentEndValue + valueDelta), 0);

        if (!currentValue.NearlyEquals(newValue))
        {
            Value = newValue;
            EndValue = newEndValue;
        }
    }
}