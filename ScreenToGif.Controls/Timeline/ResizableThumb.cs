using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenToGif.Controls.Timeline;

[TemplatePart(Name = LeftThumbId, Type = typeof(Thumb))]
[TemplatePart(Name = RightThumbId, Type = typeof(Thumb))]
public class ResizableThumb : Thumb
{
    #region Constants

    private const string LeftThumbId = "LeftThumb";
    private const string RightThumbId = "RightThumb";

    #endregion

    private Thumb _leftThumb;
    private Thumb _rightThumb;

    #region Events

    /// <summary>
    ///     Event fires when user press mouse's left button on the thumb.
    /// </summary>
    public static readonly RoutedEvent LeftDragStartedEvent = EventManager.RegisterRoutedEvent(nameof(LeftDragStarted), RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(ResizableThumb));

    /// <summary>
    ///     Event fires when the thumb is in a mouse capture state and the user moves the mouse around.
    /// </summary>
    public static readonly RoutedEvent LeftDragDeltaEvent = EventManager.RegisterRoutedEvent(nameof(LeftDragDelta), RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(ResizableThumb));

    /// <summary>
    ///     Event fires when user released mouse's left button or when CancelDrag method is called.
    /// </summary>
    public static readonly RoutedEvent LeftDragCompletedEvent = EventManager.RegisterRoutedEvent(nameof(LeftDragCompleted), RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(ResizableThumb));

    /// <summary>
    ///     Event fires when user press mouse's left button on the thumb.
    /// </summary>
    public static readonly RoutedEvent RightDragStartedEvent = EventManager.RegisterRoutedEvent(nameof(RightDragStarted), RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(ResizableThumb));

    /// <summary>
    ///     Event fires when the thumb is in a mouse capture state and the user moves the mouse around.
    /// </summary>
    public static readonly RoutedEvent RightDragDeltaEvent = EventManager.RegisterRoutedEvent(nameof(RightDragDelta), RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(ResizableThumb));

    /// <summary>
    ///     Event fires when user released mouse's left button or when CancelDrag method is called.
    /// </summary>
    public static readonly RoutedEvent RightDragCompletedEvent = EventManager.RegisterRoutedEvent(nameof(RightDragCompleted), RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(ResizableThumb));
    
    /// <summary>
    /// Add / Remove DragStartedEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragStartedEventHandler LeftDragStarted
    {
        add => AddHandler(LeftDragStartedEvent, value);
        remove => RemoveHandler(LeftDragStartedEvent, value);
    }

    /// <summary>
    /// Add / Remove DragDeltaEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragDeltaEventHandler LeftDragDelta
    {
        add => AddHandler(LeftDragDeltaEvent, value);
        remove => RemoveHandler(LeftDragDeltaEvent, value);
    }

    /// <summary>
    /// Add / Remove DragCompletedEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragCompletedEventHandler LeftDragCompleted
    {
        add => AddHandler(LeftDragCompletedEvent, value);
        remove => RemoveHandler(LeftDragCompletedEvent, value);
    }

    /// <summary>
    /// Add / Remove DragStartedEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragStartedEventHandler RightDragStarted
    {
        add => AddHandler(RightDragStartedEvent, value);
        remove => RemoveHandler(RightDragStartedEvent, value);
    }

    /// <summary>
    /// Add / Remove DragDeltaEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragDeltaEventHandler RightDragDelta
    {
        add => AddHandler(RightDragDeltaEvent, value);
        remove => RemoveHandler(RightDragDeltaEvent, value);
    }

    /// <summary>
    /// Add / Remove DragCompletedEvent handler
    /// </summary>
    [Category("Behavior")]
    public event DragCompletedEventHandler RightDragCompleted
    {
        add => AddHandler(RightDragCompletedEvent, value);
        remove => RemoveHandler(RightDragCompletedEvent, value);
    }


    #endregion

    static ResizableThumb()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableThumb), new FrameworkPropertyMetadata(typeof(ResizableThumb)));
    }

    public override void OnApplyTemplate()
    {
        _leftThumb = GetTemplateChild(LeftThumbId) as Thumb;
        _rightThumb = GetTemplateChild(RightThumbId) as Thumb;

        if (_leftThumb != null)
        {
            _leftThumb.DragStarted += RaiseLeftDragStarted;
            _leftThumb.DragDelta += RaiseLeftDragDelta;
            _leftThumb.DragCompleted += RaiseLeftDragCompleted;
        }

        if (_rightThumb != null)
        {
            _rightThumb.DragStarted += RaiseRightDragStarted;
            _rightThumb.DragDelta += RaiseRightDragDelta;
            _rightThumb.DragCompleted += RaiseRightDragCompleted;
        }
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (arrangeBounds.Width < 50)
        {
            if (_leftThumb != null)
            {
                _leftThumb.Width = 2;
                _leftThumb.Margin = new Thickness(-2, 2, 0, 2);
            }

            if (_rightThumb != null)
            {
                _rightThumb.Width = 2;
                _rightThumb.Margin = new Thickness(0, 2, -2, 2);
            }
        }
        else
        {
            if (_leftThumb != null)
            {
                _leftThumb.Width = 7;
                _leftThumb.Margin = new Thickness(-3.5, 2, 0, 2);
            }

            if (_rightThumb != null)
            {
                _rightThumb.Width = 7;
                _rightThumb.Margin = new Thickness(0, 2, -3.5, 2);
            }
        }

        return base.ArrangeOverride(arrangeBounds);
    }

    private void RaiseLeftDragStarted(object sender, DragStartedEventArgs args)
    {
        if (LeftDragStartedEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = LeftDragStartedEvent;

        RaiseEvent(args);
    }

    private void RaiseLeftDragDelta(object sender, DragDeltaEventArgs args)
    {
        if (LeftDragDeltaEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = LeftDragDeltaEvent;

        RaiseEvent(args);
    }

    private void RaiseLeftDragCompleted(object sender, DragCompletedEventArgs args)
    {
        if (LeftDragCompletedEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = LeftDragCompletedEvent;

        RaiseEvent(args);
    }

    private void RaiseRightDragStarted(object sender, DragStartedEventArgs args)
    {
        if (RightDragStartedEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = RightDragStartedEvent;

        RaiseEvent(args);
    }

    private void RaiseRightDragDelta(object sender, DragDeltaEventArgs args)
    {
        if (RightDragDeltaEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = RightDragDeltaEvent;

        RaiseEvent(args);
    }

    private void RaiseRightDragCompleted(object sender, DragCompletedEventArgs args)
    {
        if (RightDragCompletedEvent == null || !IsLoaded)
            return;

        args.Source = sender;
        args.RoutedEvent = RightDragCompletedEvent;

        RaiseEvent(args);
    }
}