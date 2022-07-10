using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Util.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline;

internal class HorizontalTrackPreviewRenderer : FrameworkElement
{
    #region Properties

    public static readonly DependencyProperty TracksProperty = DependencyProperty.Register(nameof(Tracks), typeof(ObservableCollection<ITrack>), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(default(TimeSpan?), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(default(TimeSpan?), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty NextViewportStartProperty = DependencyProperty.Register(nameof(NextViewportStart), typeof(TimeSpan?), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(default(TimeSpan?), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty NextViewportEndProperty = DependencyProperty.Register(nameof(NextViewportEnd), typeof(TimeSpan?), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(default(TimeSpan?), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty CurrentBrushProperty = DependencyProperty.Register(nameof(CurrentBrush), typeof(Brush), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty NextViewportBrushProperty = DependencyProperty.Register(nameof(NextViewportBrush), typeof(Brush), typeof(HorizontalTrackPreviewRenderer), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(50, 100, 100, 255)), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The current tracks of the project.
    /// The tracks and its sequences are used to create a mini-preview to be displayed as the background for the scrollbar.
    /// </summary>
    public ObservableCollection<ITrack> Tracks
    {
        get => (ObservableCollection<ITrack>)GetValue(TracksProperty);
        set => SetValue(TracksProperty, value);
    }

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
        get => (TimeSpan?)GetValue(SelectionStartProperty);
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

    public Brush CurrentBrush
    {
        get => (Brush)GetValue(CurrentBrushProperty);
        set => SetValue(CurrentBrushProperty, value);
    }

    public Brush SelectionBrush
    {
        get => (Brush)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public Brush NextViewportBrush
    {
        get => (Brush)GetValue(NextViewportBrushProperty);
        set => SetValue(NextViewportBrushProperty, value);
    }

    #endregion

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (Tracks == null || Tracks.Count == 0)
        {
            base.OnRender(drawingContext);
            return;
        }
        
        var maximum = Tracks.SelectMany(m => m.Sequences.Select(b => b.EndTime)).Max().TotalMilliseconds;
        var verticalCount = Math.Max(5d, Tracks.Count);
        var verticalStep = ActualHeight / verticalCount;

        //Draw tracks and sequences.
        foreach (var track in Tracks)
        {
            foreach (var sequence in track.Sequences)
            {
                drawingContext.DrawDrawing(new GeometryDrawing
                {
                    Brush = track.Accent,
                    Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, sequence.StartTime.TotalMilliseconds, null) / 100 * ActualWidth, verticalStep * track.Id - 1 - verticalStep / 2,
                        MathExtensions.CrossMultiplication(maximum, (sequence.EndTime - sequence.StartTime).TotalMilliseconds, null) / 100 * ActualWidth, verticalStep - verticalStep / 2))
                });
            }
        }

        ////Temporary: Draw extra tracks.
        //drawingContext.DrawDrawing(new GeometryDrawing
        //{
        //    Brush = Brushes.DarkMagenta,
        //    Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, 800, null) / 100 * ActualWidth, verticalStep * 3 - verticalStep / 2,
        //        MathExtensions.CrossMultiplication(maximum, 1000, null) / 100 * ActualWidth, verticalStep - verticalStep / 2))
        //});

        //drawingContext.DrawDrawing(new GeometryDrawing
        //{
        //    Brush = Brushes.DarkSeaGreen,
        //    Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, 2467, null) / 100 * ActualWidth, verticalStep * 4 - verticalStep / 2,
        //        MathExtensions.CrossMultiplication(maximum, 1400, null) / 100 * ActualWidth, verticalStep - verticalStep / 2))
        //});

        //drawingContext.DrawDrawing(new GeometryDrawing
        //{
        //    Brush = Brushes.DarkSlateGray,
        //    Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, 600, null) / 100 * ActualWidth, verticalStep * 5 - verticalStep / 2,
        //        MathExtensions.CrossMultiplication(maximum, 780, null) / 100 * ActualWidth, verticalStep - verticalStep / 2))
        //});

        //Draw current position.
        drawingContext.DrawDrawing(new GeometryDrawing
        {
            Brush = CurrentBrush,
            Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, Current.TotalMilliseconds, null) / 100 * ActualWidth, 0, 1, ActualHeight))
        });

        //Draw selection, if any.
        if (SelectionStart.HasValue && SelectionEnd.HasValue)
        {
            drawingContext.DrawDrawing(new GeometryDrawing
            {
                Brush = SelectionBrush,
                Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, SelectionStart.Value.TotalMilliseconds, null) / 100 * ActualWidth, 0,
                    MathExtensions.CrossMultiplication(maximum, (SelectionEnd.Value - SelectionStart.Value).TotalMilliseconds, null) / 100 * ActualWidth, ActualHeight))
            });
        }

        //Draw selection, if any.
        if (NextViewportStart.HasValue && NextViewportEnd.HasValue)
        {
            drawingContext.DrawDrawing(new GeometryDrawing
            {
                Brush = NextViewportBrush,
                Geometry = new RectangleGeometry(new Rect(MathExtensions.CrossMultiplication(maximum, NextViewportStart.Value.TotalMilliseconds, null) / 100 * ActualWidth, 0,
                    MathExtensions.CrossMultiplication(maximum, (NextViewportEnd.Value - NextViewportStart.Value).TotalMilliseconds, null) / 100 * ActualWidth, ActualHeight))
            });
        }

        base.OnRender(drawingContext);
    }
}