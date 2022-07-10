using ScreenToGif.Domain.ViewModels;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class StartupViewModel : BaseViewModel
{
    public RoutedUICommand NewScreenRecordingCommand { get; set; } = new()
    {
        Text = "S.Command.NewRecording",
        InputGestures = { new KeyGesture(Key.N, ModifierKeys.Control) }
    };
}