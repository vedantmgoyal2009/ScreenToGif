using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Util.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ScreenToGif.Controls.Timeline;

[TemplatePart(Name = ResizableThumbId, Type = typeof(ResizableThumb))]
[TemplatePart(Name = SimplifiedTrackId, Type = typeof(SimplifiedTrack))]
[TemplatePart(Name = HorizontalTrackPreviewRendererId, Type = typeof(HorizontalTrackPreviewRenderer))]
public class HorizontalTimelineScrollBar : RangeBase
{
    private const string ResizableThumbId = "ResizableThumb";
    private const string SimplifiedTrackId = "SimplifiedTrack";
    private const string HorizontalTrackPreviewRendererId = "HorizontalTrackPreviewRenderer";

    private ResizableThumb _resizableThumb;
    private SimplifiedTrack _simplifiedTrack;
    private HorizontalTrackPreviewRenderer _horizontalTrackPreviewRenderer;

    private Point? _firstZoomToSelectionPosition;
    private TimeSpan? _firstZoomToSelectionValue;
    private bool _skip;

    #region Properties

    public static readonly DependencyProperty TracksProperty = DependencyProperty.Register(nameof(Tracks), typeof(ObservableCollection<ITrack>), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(null, Tracks_PropertyChanged));
    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(nameof(EndValue), typeof(double), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(double), Value_PropertyChanged));
    public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan), Position_PropertyChanged));
    public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan), Position_PropertyChanged));
    public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan)));
    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    public static readonly DependencyProperty NextViewportStartProperty = DependencyProperty.Register(nameof(NextViewportStart), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    public static readonly DependencyProperty NextViewportEndProperty = DependencyProperty.Register(nameof(NextViewportEnd), typeof(TimeSpan?), typeof(HorizontalTimelineScrollBar), new PropertyMetadata(default(TimeSpan?)));
    
    /// <summary>
    /// The current tracks of the project.
    /// The tracks and its sequences are used to create a mini-preview to be displayed as the background for the scrollbar.
    /// </summary>
    public ObservableCollection<ITrack> Tracks
    {
        get => (ObservableCollection<ITrack>)GetValue(TracksProperty);
        set => SetValue(TracksProperty, value);
    }

    public double EndValue
    {
        get => (double)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
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

    public TimeSpan? NextViewportStart
    {
        get => (TimeSpan?)GetValue(NextViewportStartProperty);
        set => SetValue(NextViewportStartProperty, value);
    }

    public TimeSpan? NextViewportEnd
    {
        get => (TimeSpan?)GetValue(NextViewportEndProperty);
        set => SetValue(NextViewportEndProperty, value);
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
        SetCurrentValue(TracksProperty, new ObservableCollection<ITrack>());
        SetCurrentValue(MaximumProperty, 100d);
        SetCurrentValue(MinimumProperty, 0d);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        _resizableThumb = GetTemplateChild(ResizableThumbId) as ResizableThumb;
        _simplifiedTrack = GetTemplateChild(SimplifiedTrackId) as SimplifiedTrack;
        _horizontalTrackPreviewRenderer = GetTemplateChild(HorizontalTrackPreviewRendererId) as HorizontalTrackPreviewRenderer;

        //Listens to changes in size for the thumb, which means chinging the Value/EndValue properties.
        if (_resizableThumb != null)
        {
            _resizableThumb.LeftDragDelta += LeftThumb_DragDelta;
            _resizableThumb.RightDragDelta += RightThumb_DragDelta;
        }

        if (Tracks != null)
        {
            Tracks.CollectionChanged += (_, _) =>
            {
                Minimum = 0;
                Maximum = Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;

                _horizontalTrackPreviewRenderer.InvalidateVisual();
            };
        }

        ValueProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(Value_PropertyChanged));
        MaximumProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(Value_PropertyChanged));
        MinimumProperty.OverrideMetadata(typeof(HorizontalTimelineScrollBar), new FrameworkPropertyMetadata(Value_PropertyChanged));
        
        EventManager.RegisterClassHandler(typeof(HorizontalTimelineScrollBar), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
    }

    /// <summary>
    /// Move to point.
    /// </summary>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        //Move selection to new position, but only if click happened in the track and not in the Thumb.
        if (_simplifiedTrack.Thumb is { IsMouseOver: true })
            return;
        
        //Move Thumb to the mouse location.
        var position = e.MouseDevice.GetPosition(_simplifiedTrack);
        var positionValue = _simplifiedTrack.ValueFromPoint(position);

        //New Thumb location is halfway between target Value/EndValue (or nearby).
        var targetValue = positionValue - ((EndValue - Value) / 2);
        var newValue = Math.Max(Math.Min(Maximum - (EndValue - Value), targetValue), 0);
        var newEndValue = Math.Max(Math.Min(Maximum, newValue + (EndValue - Value)), 0);

        Value = newValue;
        EndValue = newEndValue;

        e.Handled = true;

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (!CaptureMouse())
            return;

        _firstZoomToSelectionPosition = e.GetPosition(this);
        _firstZoomToSelectionValue = TimeSpan.FromMilliseconds(_simplifiedTrack.ValueFromPoint(_firstZoomToSelectionPosition.Value));
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (!IsMouseCaptured || !_firstZoomToSelectionPosition.HasValue || !_firstZoomToSelectionValue.HasValue)
            return;

        var newPosition = e.GetPosition(this);
        var newValue = _simplifiedTrack.ValueFromPoint(newPosition);

        if (newPosition.X > _firstZoomToSelectionPosition.Value.X)
        {
            NextViewportStart = _firstZoomToSelectionValue;
            NextViewportEnd = TimeSpan.FromMilliseconds(newValue);
        }
        else
        {
            NextViewportStart = TimeSpan.FromMilliseconds(newValue);
            NextViewportEnd = _firstZoomToSelectionValue.Value;
        }
    }

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured)
            return;

        ReleaseMouseCapture();

        if (NextViewportStart.HasValue && NextViewportEnd.HasValue)
        {
            //TODO: Validate max/min.

            ViewportStart = NextViewportStart.Value;
            ViewportEnd = NextViewportEnd.Value;
        }

        NextViewportStart = null;
        NextViewportEnd = null;

        _firstZoomToSelectionPosition = null;
        _firstZoomToSelectionValue = null;
    }

    //TODO: OnMouseRightButtonDown/Up Select to change viewport start/end.

    private static void Value_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        if (scrollBar.Tracks?.Any() == true)
        {
            scrollBar.Minimum = 0;
            scrollBar.Maximum = scrollBar.Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;

            //When new or surpassing the maximum.
            if (scrollBar.EndValue.NearlyEquals(0) || scrollBar.EndValue > scrollBar.Maximum)
                scrollBar.EndValue = scrollBar.Maximum;
        }
        
        //scrollBar.RenderBackground();
        scrollBar.CalculateScroll();
    }

    private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is not HorizontalTimelineScrollBar scrollBar)
            return;

        scrollBar.UpdateValue(e.HorizontalChange, e.VerticalChange);
    }

    private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        e.Handled = true;
        
        SetPosition(Value + e.HorizontalChange, EndValue);
    }

    private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        e.Handled = true;

        SetPosition(Value, EndValue + e.HorizontalChange, true);
    }

    private void CalculateScroll()
    {
        if (_skip || Tracks == null || Tracks.Count == 0)
            return;

        _skip = true;

        Value = ViewportStart.TotalMilliseconds;
        EndValue = ViewportEnd.TotalMilliseconds;

        _skip = false;
    }

    private void InterpretPosition()
    {
        if (_skip)
            return;

        _skip = true;

        ViewportStart = TimeSpan.FromMilliseconds(Value);
        ViewportEnd = TimeSpan.FromMilliseconds(EndValue);

        _skip = false;
    }

    private void SetPosition(double value, double endValue, bool isFromRight = false)
    {
        if (isFromRight)
        {
            endValue = Math.Round(Math.Min(Maximum, Math.Max(value + 10, endValue)), 0);
            value = Math.Round(Math.Max(Minimum, Math.Min(value, endValue - 10)), 0);
        }
        else
        {
            value = Math.Round(Math.Max(Minimum, Math.Min(value, endValue - 10)), 0);
            endValue = Math.Round(Math.Min(Maximum, Math.Max(value + 10, endValue)), 0);
        }

        //Minimum span of 100ms.
        if (endValue - value < 100)
            return;

        var perPixel = Maximum / _simplifiedTrack.ActualWidth;

        //Limit by thumb width as well, 10px.
        if ((endValue - value) / perPixel < 10)
            return;

        Value = value;
        EndValue = endValue;
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

        //Clamp down delta to not alter the view span when dragging beyond the limits.
        if (valueDelta < 0)
            valueDelta = Value + valueDelta < Minimum ? (Value - Minimum) * -1 : valueDelta;
        else
            valueDelta = EndValue + valueDelta > Maximum ? (Maximum - EndValue) : valueDelta;

        SetPosition(Value + valueDelta, EndValue + valueDelta);
    }
}