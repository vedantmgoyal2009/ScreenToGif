using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Dialogs;
using ScreenToGif.Util;
using ScreenToGif.Util.InterProcessChannel;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScreenToGif;

public partial class App : Application
{
    private Mutex _mutex;
    private bool _accepted;
    private readonly List<Exception> _exceptionList = new();
    private static readonly object Lock = new();

    internal static NotifyIcon NotifyIcon { get; private set; }

    public static AppViewModel ViewModel { get; } = new();

    private async void App_Startup(object sender, StartupEventArgs e)
    {
        //Handle unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;

        //Increases the duration of the tooltip display.
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

        SecurityHelper.SetSecurityProtocol();

        Arguments.Prepare(e.Args);

        //Render mode.
        RenderOptions.ProcessRenderMode = UserSettings.All.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;

        await LocalizationHelper.SelectCulture(UserSettings.All.LanguageCode);
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        ThemeHelper.SelectGridTheme();

        //Listen to changes in theme.
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        #region Download mode

        if (Arguments.IsInDownloadMode)
        {
            //TODO: Implement downloader.
            //var downloader = new Downloader
            //{
            //    DownloadMode = Arguments.DownloadMode,
            //    DestinationPath = Arguments.DownloadPath
            //};
            //downloader.ShowDialog();

            Environment.Exit(90);
            return;
        }

        #endregion

        #region Settings persistence mode

        if (Arguments.IsInSettingsMode)
        {
            SettingsPersistenceChannel.RegisterServer();
            return;
        }

        #endregion

        #region If set, it allows only one instance per user

        //The singleton works on a per-user and per-executable mode.
        //Meaning that a different user and/or a different executable instances can co-exist.
        //Part of this code won't work on debug mode, since the SetForegroundWindow() needs focus on the foreground window calling the method.
        if (UserSettings.All.SingleInstance && !Arguments.NewInstance)
        {
            try
            {
                using (var thisProcess = Process.GetCurrentProcess())
                {
                    var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
                    var name = thisProcess.MainModule?.FileName ?? Assembly.GetEntryAssembly()?.Location ?? "ScreenToGif";
                    var location = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
                    var mutexName = (user?.Value ?? Environment.UserName) + "_" + location;

                    _mutex = new Mutex(true, mutexName, out _accepted);

                    //If the mutext failed to be accepted, it means that another process already opened it.
                    if (!_accepted)
                    {
                        var warning = true;

                        //Switch to the other app (get only one, if multiple available). Use name of assembly.
                        using (var process = Process.GetProcessesByName(thisProcess.ProcessName).FirstOrDefault(f => f.MainWindowHandle != thisProcess.MainWindowHandle))
                        {
                            if (process != null)
                            {
                                var handles = Util.Native.Windows.GetWindowHandlesFromProcess(process);

                                //Show the window before setting focus.
                                Native.External.User32.ShowWindow(handles.Count > 0 ? handles[0] : process.Handle, Domain.Enums.Native.ShowWindowCommands.Show);

                                //Set user the focus to the window.
                                Native.External.User32.SetForegroundWindow(handles.Count > 0 ? handles[0] : process.Handle);
                                warning = false;

                                InstanceSwitcherChannel.SendMessage(process.Id, e.Args);
                            }
                        }

                        //If no window available (app is in the system tray), display a warning.
                        //if (warning) //TODO
                        //    Dialog.Ok(LocalizationHelper.Get("S.Warning.Single.Title"), LocalizationHelper.Get("S.Warning.Single.Header"), LocalizationHelper.Get("S.Warning.Single.Message"), Icons.Info);

                        Environment.Exit(0);
                        return;
                    }

                    //If this is the first instance, register the inter process channel to listen for other instances.
                    InstanceSwitcherChannel.RegisterServer(InstanceSwitch_Received);
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Impossible to check if another instance is running");
            }
        }

        #endregion

        RegisterNotifyIconCommands();
        RegisterViewModelCommands();
        RegisterShortcuts();

        new DownloadDialog().ShowDialog(); return;
        //ErrorDialog.Show("S.Dialog.Error.Header", "S.Dialog.Error.Info", new ApplicationException("Nothing else to do", new AccessViolationException("You can't access that memory"))); return;
        //ErrorDialog.ShowStatic("Something bad happened", "We were not expecting it.", new Exception()); return;
        
        //TODO: Tasks.
#pragma warning disable CS4014
        //Task.Factory.StartNew(ViewModel.ClearTemporaryFiles, TaskCreationOptions.LongRunning);
        //Task.Factory.StartNew(ViewModel.CheckForUpdates, TaskCreationOptions.LongRunning);
        //Task.Factory.StartNew(ViewModel.SendFeedback, TaskCreationOptions.LongRunning);
#pragma warning restore CS4014

        #region Startup

        if (Arguments.Open)
        {
            ViewModel.LaunchCommand.Execute(Arguments.WindownToOpen);
            return;
        }

        var startup = UserSettings.All.StartUp;

        if (UserSettings.All.StartMinimized)
            startup = -1;

        //If files are being sent via parameter, force the editor to open.
        if (Arguments.FileNames.Any())
            startup = 4;

        ViewModel.LaunchCommand.Execute(startup);

        #endregion
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ShowException(e.Exception, "Dispatcher Unhandled");

        e.Handled = true;
    }

    private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception)
            return;

        ShowException(exception, "Unhandled");
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
            return;

        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        ThemeHelper.SelectGridTheme();
    }

    internal static void InstanceSwitch_Received(object _, InstanceSwitcherMessage message)
    {
        try
        {
            var args = message.Args;

            if (args?.Length > 0)
                Arguments.Prepare(args);

            //if (Arguments.Open)
            //    OpenExecute(Arguments.WindownToOpen, true);
            //else
            //    OpenExecute(UserSettings.All.StartUp);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Unable to execute arguments from IPC.");
        }
    }

    private void ShowException(Exception e, string source)
    {
        LogWriter.Log(e, source);

        lock (Lock)
        {
            try
            {

                //Avoid displaying an exception that is already being displayed.
                if (_exceptionList.Any(a => a.Message == e.Message))
                    return;

                //Adding to the list, so a second exception with the same name won't be displayed.
                _exceptionList.Add(e);

                Current.Dispatcher.Invoke(() =>
                {
                    ErrorDialog.Show("S.Dialog.Error.Header", "S.Dialog.Error.Info", e);
                });
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Impossible to display the error.");
            }
            finally
            {
                //By removing the exception, the same exception can be displayed later.
                _exceptionList.Remove(e);
            }
        }
    }

    private void RegisterNotifyIconCommands()
    {
        NotifyIcon = (NotifyIcon)FindResource("NotifyIcon");

        if (NotifyIcon == null)
            return;

        NotifyIcon.DataContext = ViewModel;
        NotifyIcon.Visibility = UserSettings.All.ShowNotificationIcon || UserSettings.All.StartMinimized ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RegisterViewModelCommands()
    {
        //https://stackoverflow.com/a/30686620/1735672
        //CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ViewModel.NewScreenRecordingCommand, OpenScreenRecorder, CanOpenRecorder));

        ViewModel.LaunchCommand = new RelayCommand(Launch);

        ViewModel.StartupCommand = new RelayCommand(OpenStartup);
        ViewModel.EditorCommand = new RelayCommand(OpenEditor);
        ViewModel.OptionsCommand = new RelayCommand(OpenOptions);
    }

    internal void RegisterShortcuts()
    {
        //Registers all shortcuts.
        var screen = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.RecorderModifiers, UserSettings.All.RecorderShortcut, () => TryOpeningScreenRecorder(true), true);
        var webcam = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.WebcamRecorderModifiers, UserSettings.All.WebcamRecorderShortcut, () => TryOpeningWebcamRecorder(true), true);
        var board = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.BoardRecorderModifiers, UserSettings.All.BoardRecorderShortcut, () => TryOpeningBoardRecorder(true), true);
        var editor = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.EditorModifiers, UserSettings.All.EditorShortcut, () => TryOpeningEditor(true), true);
        var options = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.OptionsModifiers, UserSettings.All.OptionsShortcut, () => TryOpeningOptions(true), true);
        var exit = HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.ExitModifiers, UserSettings.All.ExitShortcut, () => TryExiting(true), true);

        //Updates the input gesture text of each command.
        //MainViewModelOld.RecorderGesture = screen ? Other.GetSelectKeyText(UserSettings.All.RecorderShortcut, UserSettings.All.RecorderModifiers, true, true) : "";
        //MainViewModelOld.WebcamRecorderGesture = webcam ? Other.GetSelectKeyText(UserSettings.All.WebcamRecorderShortcut, UserSettings.All.WebcamRecorderModifiers, true, true) : "";
        //MainViewModelOld.BoardRecorderGesture = board ? Other.GetSelectKeyText(UserSettings.All.BoardRecorderShortcut, UserSettings.All.BoardRecorderModifiers, true, true) : "";
        //MainViewModelOld.EditorGesture = editor ? Other.GetSelectKeyText(UserSettings.All.EditorShortcut, UserSettings.All.EditorModifiers, true, true) : "";
        //MainViewModelOld.OptionsGesture = options ? Other.GetSelectKeyText(UserSettings.All.OptionsShortcut, UserSettings.All.OptionsModifiers, true, true) : "";
        //MainViewModelOld.ExitGesture = exit ? Other.GetSelectKeyText(UserSettings.All.ExitShortcut, UserSettings.All.ExitModifiers, true, true) : "";
    }
}