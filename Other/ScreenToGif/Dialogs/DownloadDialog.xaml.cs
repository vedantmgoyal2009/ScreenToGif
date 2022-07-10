using ScreenToGif.Controls;
using System.Windows;

namespace ScreenToGif.Dialogs;

public partial class DownloadDialog : ExWindow
{
    public DownloadDialog()
    {
        InitializeComponent();
    }
    
    //TODO: Show details, etc.

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}