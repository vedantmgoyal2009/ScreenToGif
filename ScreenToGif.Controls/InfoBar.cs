using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class InfoBar : Control
{
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(StatusTypes), typeof(InfoBar), new PropertyMetadata(default(StatusTypes)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoBar), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(InfoBar), new PropertyMetadata(default(string)));
    
    public StatusTypes Type
    {
        get => (StatusTypes)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Button ActionButton { get; set; }

    static InfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
    }
}