using ScreenToGif.Controls.Recorder;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.Editor;
using ScreenToGif.Windows.Other;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Windows;

public partial class EditorEx : Window
{
    #region Variables

    /// <summary>
    /// Lock used to prevent firing multiple times (at the same time) both the Activated/Deactivated events.
    /// </summary>
    public static readonly object ActivateLock = new();

    private readonly EditorViewModel _viewModel;

    #endregion

    public bool HasProjectLoaded => _viewModel?.HasProject == true;
    
    public EditorEx()
    {
        InitializeComponent();

        DataContext = _viewModel = new EditorViewModel();

        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.NewScreenRecordingCommand, NewScreenRecorderExecuted, (_, e) => e.CanExecute = !_viewModel.IsLoading && !e.Handled && Application.Current.Windows.OfType<Window>().All(a => a is not BaseRecorder)),
            new CommandBinding(_viewModel.ExportCommand, ExportExecuted, _viewModel.ExportCanExecute),
        });
    }
    
    //Editor:
    //  Ribbon:
    //      File:
    //          New (screen recording, webcam recording, board recording, empty project [no tracks])
    //          Insert (screen recording, webcam recording, board recording, media)
    //          File (save, export)
    //      Home:
    //          Action stack.
    //          Clipboard (based on selection, elements in the decorator layer or in timeline).
    //          Zoom
    //          Select
    //      Track (when selected):
    //      Sequence (when selected):
    //      SubSequence (when selected):
    //  Previewer:
    //      Zoom, pan, scroll.
    //      Decorator layer?
    //  Timeline:
    //      Tracks (layers)
    //          Sequences
    //              Sub-sequences.
    //      Scrub (current timestamp selected)
    //          Changing timestamp triggers rendering.
    //              Triggered by the change in value in the view model, not by the timeline it self.
    //      Virtualized rendering.

    //Panels:
    //Not stored directly in this window, but in user controls.
    //When a property is changed, the panel will report the event, which will trigger the rendering.

    #region Main events

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        #region Adjust the position

        //Tries to adjust the position/size of the window, centers on screen otherwise.
        if (!UpdatePositioning())
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

        #endregion
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        lock (ActivateLock)
        {
            //Returns the preview if was playing before the deactivation of the window.
            //if (WasPreviewing)
            //{
            //    WasPreviewing = false;
            //    PlayPause();
            //}
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!IsLoaded)
            return;

        lock (ActivateLock)
        {
            try
            {
                //Pauses the recording preview.
                //if (_timerPreview.Enabled)
                //{
                //    WasPreviewing = true;
                //    Pause();
                //}
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Exception when losing focus on window.");
            }
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        //TODO: Stop processing, stop playback.
        //Ask to save project if not saved before.
        //  Option to ignore this question if project was exported.
        //      Option to delete this project.
        //  Option to ignore this question and just delete this project.
        //  Option to ignore this question and just leave project in cache.

        //Manually get the position/size of the window, so it's possible opening multiple instances.
        UserSettings.All.EditorTop = Top;
        UserSettings.All.EditorLeft = Left;
        UserSettings.All.EditorWidth = Width;
        UserSettings.All.EditorHeight = Height;
        UserSettings.All.EditorWindowState = WindowState;
        UserSettings.Save();
    }

    private void ShowClipboardButton_Click(object sender, RoutedEventArgs e)
    {

    }

    //Panning/Zooming events for the previewer (maybe embed this in the previewer itself).

    #endregion

    #region Command events

    private void NewScreenRecorderExecuted(object sender, ExecutedRoutedEventArgs args)
    {
        //Open Export dialog.
        //Pass ProjectViewModel to it.
        //Wait for callback.
    }
    
    private void ExportExecuted(object sender, ExecutedRoutedEventArgs args)
    {
        //Open Export dialog.

        //Pass ProjectViewModel to it.
        //Wait for callback.
    }

    #endregion

    private bool UpdatePositioning(bool onLoad = true)
    {
        //TODO: When the DPI changes, these values are still from the latest dpi.
        var top = onLoad ? UserSettings.All.EditorTop : Top;
        var left = onLoad ? UserSettings.All.EditorLeft : Left;
        var width = onLoad ? UserSettings.All.EditorWidth : Width;
        var height = onLoad ? UserSettings.All.EditorHeight : Height;
        var state = onLoad ? UserSettings.All.EditorWindowState : WindowState;

        //If the position was never set, let it center on screen.
        if (double.IsNaN(top) && double.IsNaN(left))
            return false;

        //The catch here is to get the closest monitor from current Top/Left point.
        var monitors = MonitorHelper.AllMonitorsScaled(this.GetVisualScale());
        var closest = monitors.FirstOrDefault(x => x.Bounds.Contains(new Point((int)left, (int)top))) ?? monitors.FirstOrDefault(x => x.IsPrimary);

        if (closest == null)
            return false;

        //To much to the Left.
        if (closest.WorkingArea.Left > left + width - 100)
            left = closest.WorkingArea.Left;

        //Too much to the top.
        if (closest.WorkingArea.Top > top + height - 100)
            top = closest.WorkingArea.Top;

        //Too much to the right.
        if (closest.WorkingArea.Right < left + 100)
            left = closest.WorkingArea.Right - width;

        //Too much to the bottom.
        if (closest.WorkingArea.Bottom < top + 100)
            top = closest.WorkingArea.Bottom - height;

        if (top is > int.MaxValue or < int.MinValue || left is > int.MaxValue or < int.MinValue || width is > int.MaxValue or < 0 || height is > int.MaxValue or < 0)
        {
            var desc = $"On load: {onLoad}\nScale: {this.GetVisualScale()}\n\n" +
                       $"Screen: {closest.AdapterName}\nBounds: {closest.Bounds}\n\nTopLeft: {top}x{left}\nWidthHeight: {width}x{height}\n\n" +
                       $"TopLeft Settings: {UserSettings.All.EditorTop}x{UserSettings.All.EditorLeft}\nWidthHeight Settings: {UserSettings.All.EditorWidth}x{UserSettings.All.EditorHeight}";
            LogWriter.Log("Wrong Editor window sizing", desc);
            return false;
        }

        //To eliminate the flicker of moving the window to the correct screen, hide and then show it again.
        if (onLoad)
            Opacity = 0;

        //First move the window to the final monitor, so that the UI scale can be adjusted.
        this.MoveToScreen(closest);

        Top = top;
        Left = left;
        Width = width;
        Height = height;
        WindowState = state;

        if (onLoad)
            Opacity = 1;

        return true;
    }

    public async Task LoadProject(RecordingProject project)
    {
        Activate();

        //TODO: Possible to be cancelled.

        if (project?.Any == true)
            await _viewModel.ImportFromRecording(project);
        
        Encoder.Restore();
        ShowInTaskbar = true;
        WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;
    }

    public void LoadFromArguments()
    {
        //Identify arguments
        //Validate what's comming.
        //Only load groups of the same type (media, project).
        //Load files
        //Parse data.
        //Create tracks/sequences for each file.
    }

    private void Previewer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.Seek(TimeSpan.FromMilliseconds(0));
    }
}