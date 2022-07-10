using ScreenToGif.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ScreenToGif.Dialogs;

public partial class ErrorDialog : ExWindow
{
    public Exception Exception { get; set; }

    public ErrorDialog()
    {
        InitializeComponent();
    }
    
    public static void Show(string titleId, string detailsId, Exception exception = null)
    {
        var dialog = new ErrorDialog { Exception = exception };
        dialog.SetData(titleId, detailsId);
        dialog.ShowDialog();
    }

    public static void ShowStatic(string title, string details, Exception exception = null)
    {
        var dialog = new ErrorDialog { Exception = exception };
        dialog.SetData(title, details, true);
        dialog.ShowDialog();
    }

    private void SetData(string title, string details, bool isStatic = false)
    {
        if (isStatic)
        {
            TitleTextBlock.Text = title;
            DetailsRun.Text = details;
        }
        else
        {
            TitleTextBlock.SetResourceReference(TextBlock.TextProperty, title);
            DetailsRun.SetResourceReference(Run.TextProperty, details);
        }

        if (Exception != null)
        {
            var run = new Run();
            run.SetResourceReference(Run.TextProperty, "S.Dialog.Error.SeeMore");
            run.SetResourceReference(FrameworkContentElement.ToolTipProperty, "S.Dialog.Error.SeeMore.Tooltip");

            var hyper = new ExHyperlink(run);
            hyper.RequestNavigate += ExceptionHyperlink_RequestNavigate;
            hyper.Click += ExceptionHyperlink_Click;

            DetailsTextBlock.Inlines.Add(new LineBreak());
            DetailsTextBlock.Inlines.Add(new LineBreak());
            DetailsTextBlock.Inlines.Add(hyper);
        }

        DismissButton.Focus();
    }

    private void ExceptionHyperlink_Click(object sender, RoutedEventArgs e)
    {
        ExceptionDialog.Show(Exception);
    }

    private void ExceptionHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ExceptionDialog.Show(Exception);
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void FeedbackButton_Click(object sender, RoutedEventArgs e)
    {
        FeedbackDialog.Show(Exception);
    }
}