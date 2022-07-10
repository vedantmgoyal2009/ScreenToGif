using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScreenToGif.Controls;

public class ExButton : Button
{
    #region Variables

    public static readonly DependencyProperty IsAccentedProperty = DependencyProperty.Register(nameof(IsAccented), typeof(bool), typeof(ExButton));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Brush), typeof(ExButton));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ExButton));

    public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(ExButton), new FrameworkPropertyMetadata(double.NaN));

    public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(ExButton), new FrameworkPropertyMetadata(double.NaN));

    public static readonly DependencyProperty KeyGestureProperty = DependencyProperty.Register(nameof(KeyGesture), typeof(string), typeof(ExButton), new FrameworkPropertyMetadata(""));

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(ExButton), new FrameworkPropertyMetadata(TextWrapping.NoWrap,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    #region Properties

    /// <summary>
    /// True if the Button has accent color.
    /// </summary>
    [Description("True if the Button has accent color."), Category("Common")]
    public bool IsAccented
    {
        get => (bool)GetValue(IsAccentedProperty);
        set => SetCurrentValue(IsAccentedProperty, value);
    }

    /// <summary>
    /// The icon of the button as a brush.
    /// </summary>
    [Description("The icon of the button as a brush."), Category("Common")]
    public Brush Icon
    {
        get => (Brush)GetValue(IconProperty);
        set => SetCurrentValue(IconProperty, value);
    }

    /// <summary>
    /// The text of the button.
    /// </summary>
    [Description("The text of the button."), Category("Common")]
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetCurrentValue(TextProperty, value);
    }

    /// <summary>
    /// The height of the button content.
    /// </summary>
    [Description("The height of the button content."), Category("Common")]
    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetCurrentValue(IconHeightProperty, value);
    }

    /// <summary>
    /// The width of the button content.
    /// </summary>
    [Description("The width of the button content."), Category("Common")]
    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetCurrentValue(IconWidthProperty, value);
    }

    /// <summary>
    /// The KeyGesture of the button.
    /// </summary>
    [Description("The KeyGesture of the button."), Category("Common")]
    public string KeyGesture
    {
        get => (string)GetValue(KeyGestureProperty);
        set => SetCurrentValue(KeyGestureProperty, value);
    }

    /// <summary>
    /// The TextWrapping property controls whether or not text wraps 
    /// when it reaches the flow edge of its containing block box. 
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    #endregion

    static ExButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExButton), new FrameworkPropertyMetadata(typeof(ExButton)));
    }
}