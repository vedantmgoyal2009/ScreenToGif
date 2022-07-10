using System.Windows;

namespace ScreenToGif.ViewModel.Behaviors;

public static class DialogBehavior
{
    public static readonly DependencyProperty DialogBoxProperty = DependencyProperty.RegisterAttached("DialogBox", typeof(object), typeof(DialogBehavior), new PropertyMetadata(null, OnDialogBoxChange));

    public static void SetDialogBox(DependencyObject source, object value)
    {
        source.SetValue(DialogBoxProperty, value);
    }

    public static object GetDialogBox(DependencyObject source)
    {
        return source.GetValue(DialogBoxProperty);
    }

    private static void OnDialogBoxChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window parent || e.NewValue == null)
            return;
        
        //Create an instance of the dialog box window that is keyed to this view model
        var resource = Application.Current.TryFindResource(e.NewValue.GetType());

        if (resource is Window window)
        {
            window.DataContext = e.NewValue;
            window.ShowDialog();
        }
    }
}