using System;
using System.Windows;

namespace ScreenToGif.Dialogs;

public partial class FeedbackDialog : Window
{
    public FeedbackDialog()
    {
        InitializeComponent();
    }

    public static void Show(Exception ex)
    {

    }
}