using Microsoft.UI.Xaml.Controls;

using WinUITest.ViewModels;

namespace WinUITest.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
