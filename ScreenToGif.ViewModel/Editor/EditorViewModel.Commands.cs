using ScreenToGif.Domain.ViewModels;
using System.Windows.Input;

namespace ScreenToGif.ViewModel.Editor;

public partial class EditorViewModel : BaseViewModel
{
    public RoutedUICommand NewScreenRecordingCommand { get; set; } = new()
    {
        Text = "S.Command.NewRecording",
        InputGestures = { new KeyGesture(Key.N, ModifierKeys.Control) }
    };

    public RoutedUICommand ExportCommand { get; set; } = new()
    {
        Text = "S.Command.Export",
        InputGestures = { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public void ExportCanExecute(object sender, CanExecuteRoutedEventArgs args) => args.CanExecute = !IsLoading && Project != null;
}