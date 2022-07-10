using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ScreenToGif.Controls;

public class ExHyperlink : Hyperlink
{
    private static readonly DependencyProperty IsHyperlinkPressedProperty = DependencyProperty.Register(nameof(IsHyperlinkPressed), typeof(bool), typeof(ExHyperlink),
        new FrameworkPropertyMetadata(false));

    public bool IsHyperlinkPressed => (bool)GetValue(IsHyperlinkPressedProperty);

    static ExHyperlink()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExHyperlink), new FrameworkPropertyMetadata(typeof(ExHyperlink)));
    }

    public ExHyperlink() : base()
    { }

    public ExHyperlink(Inline run): base(run)
    { }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        //It's possible that the mouse state could have changed.
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            Mouse.Capture(this);

            if (IsMouseCaptured)
            {
                //CaptureMouse could end up changing the state..
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    SetValue(IsHyperlinkPressedProperty, true);
                }
                else
                {
                    //Release capture since we decided not to press the button.
                    ReleaseMouseCapture();
                }
            }
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
            ReleaseMouseCapture();

        if ((bool)GetValue(IsHyperlinkPressedProperty))
            SetValue(IsHyperlinkPressedProperty, false);

        base.OnMouseLeftButtonUp(e);
    }
}
