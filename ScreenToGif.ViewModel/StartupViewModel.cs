using ScreenToGif.Domain.ViewModels;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class StartupViewModel : BaseViewModel
{
    public RoutedUICommand ScreenRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewRecording",
        InputGestures = { new KeyGesture(Key.N, ModifierKeys.Control) }
    };

    public RoutedUICommand WebcamRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewWebcamRecording",
        InputGestures = { new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift) }
    };

    public RoutedUICommand BoardRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewBoardRecording",
        InputGestures = { new KeyGesture(Key.B, ModifierKeys.Control | ModifierKeys.Shift) }
    };

    public RoutedUICommand EditorCommand { get; set; } = new()
    {
        Text = "S.Command.Editor",
        InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public RoutedUICommand OptionsCommand { get; set; } = new()
    {
        Text = "S.Command.Options",
        InputGestures = { new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public RoutedUICommand UpdateCommand { get; set; } = new()
    {
        Text = "S.Command.Options",
        InputGestures = { new KeyGesture(Key.U, ModifierKeys.Control | ModifierKeys.Alt) }
    };
}