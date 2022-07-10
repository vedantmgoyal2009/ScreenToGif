using ScreenToGif.Controls;
using ScreenToGif.Util;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ScreenToGif.Dialogs;

public partial class ExceptionDialog : ExWindow
{
    public Exception Exception { get; set; }

    public ExceptionDialog()
    {
        InitializeComponent();
    }

    private void SetData(Exception exception)
    {
        Exception = exception;

        var current = Exception;

        while (current != null)
        {
            DetailsTextBlock.Inlines.Add(new Run(current.Message) { FontWeight = FontWeights.SemiBold });
            DetailsTextBlock.Inlines.Add(new LineBreak());
            DetailsTextBlock.Inlines.Add(new Run(current.StackTrace));

            current = current.InnerException;

            if (current != null)
            {
                DetailsTextBlock.Inlines.Add(new LineBreak());
                DetailsTextBlock.Inlines.Add(new Separator { Height = 1 });
                DetailsTextBlock.Inlines.Add(new LineBreak());
            }
        }

        DismissButton.Focus();
    }

    public static void Show(Exception exception)
    {
        var dialog = new ExceptionDialog();
        dialog.SetData(exception);
        dialog.ShowDialog();
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        await Exception.AsText().CopyToClipboard();
    }

    private void FeedbackButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO: Open Feedback tool.
        //FeedbackDialog.Show(Exception);
    }
}