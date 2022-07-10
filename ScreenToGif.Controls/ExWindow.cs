using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Native.External;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Native.Structs;
using ScreenToGif.Util;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace ScreenToGif.Controls;

[TemplatePart(Name = NonClientAreaElementId, Type = typeof(UIElement))]
[TemplatePart(Name = MinimizeButtonId, Type = typeof(Button))]
[TemplatePart(Name = MaximizeButtonId, Type = typeof(Button))]
[TemplatePart(Name = RestoreButtonId, Type = typeof(Button))]
[TemplatePart(Name = CloseButtonId, Type = typeof(Button))]
public class ExWindow : Window
{
    private const string NonClientAreaElementId = "NonClientAreaElement";
    private const string MinimizeButtonId = "MinimizeButton";
    private const string MaximizeButtonId = "MaximizeButton";
    private const string RestoreButtonId = "RestoreButton";
    private const string CloseButtonId = "CloseButton";

    private UIElement _nonClientAreaElement;
    private Button _minimizeButton;
    private Button _maximizeButton;
    private Button _restoreButton;
    private Button _closeButton;

    public static readonly DependencyProperty ExtendIntoTitleBarProperty = DependencyProperty.Register(nameof(ExtendIntoTitleBar), typeof(bool), typeof(ExWindow), new PropertyMetadata(true));
    public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(ExWindow), new PropertyMetadata(true, ShowMinimizeButton_PropertyChanged));
    public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(nameof(ShowMaximizeButton), typeof(bool), typeof(ExWindow), new PropertyMetadata(true, ShowMaximizeButton_PropertyChanged));
    public static readonly DependencyProperty WindowSystemBackdropProperty = DependencyProperty.Register(nameof(WindowSystemBackdrop), typeof(SystemBackdropTypes), typeof(ExWindow), new PropertyMetadata(SystemBackdropTypes.Main, WindowBackdrop_PropertyChanged));

    public bool ExtendIntoTitleBar
    {
        get => (bool)GetValue(ExtendIntoTitleBarProperty);
        set => SetValue(ExtendIntoTitleBarProperty, value);
    }

    public bool ShowMinimizeButton
    {
        get => (bool)GetValue(ShowMinimizeButtonProperty);
        set => SetValue(ShowMinimizeButtonProperty, value);
    }

    public bool ShowMaximizeButton
    {
        get => (bool)GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    public SystemBackdropTypes WindowSystemBackdrop
    {
        get => (SystemBackdropTypes)GetValue(WindowSystemBackdropProperty);
        set => SetValue(WindowSystemBackdropProperty, value);
    }

    static ExWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExWindow), new FrameworkPropertyMetadata(typeof(ExWindow)));
    }

    public ExWindow()
    {
        var chrome = new WindowChrome
        {
            CaptionHeight = 32,
            ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
            UseAeroCaptionButtons = false,
            GlassFrameThickness = new Thickness(-1)
        };

        WindowChrome.SetWindowChrome(this, chrome);

        this.SetBackdrop(WindowSystemBackdrop);
        this.SetCornerPreference(CornerPreferences.Round);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        this.GetHwndSource()?.AddHook(Window_Hook);
    }

    public override void OnApplyTemplate()
    {
        _nonClientAreaElement = GetTemplateChild(NonClientAreaElementId) as UIElement;
        _minimizeButton = GetTemplateChild(MinimizeButtonId) as Button;
        _maximizeButton = GetTemplateChild(MaximizeButtonId) as Button;
        _restoreButton = GetTemplateChild(RestoreButtonId) as Button;
        _closeButton = GetTemplateChild(CloseButtonId) as Button;

        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this), (_, args) => args.CanExecute = ShowMinimizeButton));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, _) => SystemCommands.MaximizeWindow(this), (_, args) => args.CanExecute = ShowMaximizeButton));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, _) => SystemCommands.RestoreWindow(this), (_, args) => args.CanExecute = ShowMaximizeButton));
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => SystemCommands.CloseWindow(this)));

        base.OnApplyTemplate();
    }

    private IntPtr Window_Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        switch ((WindowsMessages)msg)
        {
            case WindowsMessages.NonClientHitTest:
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) || !ShowMaximizeButton || ResizeMode is ResizeMode.NoResize or ResizeMode.CanMinimize)
                    return IntPtr.Zero;
                
                var x = lparam.ToInt32() & 0xffff;
                var y = lparam.ToInt32() >> 16;

                var button = WindowState == WindowState.Maximized ? _restoreButton : _maximizeButton;

                if (button.HitTestElement(x, y))
                {
                    button.SetCurrentValue(BackgroundProperty, FindResource("Brush.TitleBar.Button.Background.Hover"));

                    handled = true;
                    return new IntPtr((int)HitTestTargets.MaximizeButton);
                }
                
                button.ClearValue(BackgroundProperty);

                break;
            }

            case WindowsMessages.NonClientLeftButtonDown:
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) || !ShowMaximizeButton || ResizeMode is ResizeMode.NoResize or ResizeMode.CanMinimize)
                    return IntPtr.Zero;

                //This is necessary in order to change the background color for the maximize/restore button, since the HitTest is handled above.
                var x = lparam.ToInt32() & 0xffff;
                var y = lparam.ToInt32() >> 16;

                var button = WindowState == WindowState.Maximized ? _restoreButton : _maximizeButton;
                
                if (button.HitTestElement(x, y))
                {
                    button.SetCurrentValue(BackgroundProperty, FindResource("Brush.TitleBar.Button.Background.Pressed"));

                    //Without this, the button click near the bottom border would not work and it would display a ghost button nearby.
                    button.Command.Execute(null);
                    handled = true;
                }
                else
                    button.ClearValue(BackgroundProperty);

                break;
            }

            case WindowsMessages.GetMinMaxInfo:
            {
                var info = (MinMaxInfo) Marshal.PtrToStructure(lparam, typeof(MinMaxInfo))!;
                var monitor = WindowHelper.NearestMonitorForWindow(hwnd);

                if (monitor != IntPtr.Zero)
                {
                    var monitorInfo = new MonitorInfoEx();
                    User32.GetMonitorInfo(new HandleRef(this, monitor), monitorInfo);

                    var rcWorkArea = monitorInfo.Work;
                    var rcMonitorArea = monitorInfo.Monitor;

                    info.MaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    info.MaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    info.MaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    info.MaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(info, lparam, true);

                break;
            }

            case WindowsMessages.WindowPositionChanged:
            {
                BorderThickness = WindowState == WindowState.Maximized ? new Thickness(0) : new Thickness(1);
                break;
            }
        }

        return IntPtr.Zero;
    }

    private static void ShowMinimizeButton_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ExWindow window)
            return;

        if (window.ShowMinimizeButton)
            window.DisableMinimize();
        else
            window.EnableMinimize();
    }

    private static void ShowMaximizeButton_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ExWindow window)
            return;

        if (window.ShowMaximizeButton)
            window.DisableMaximize();
        else
            window.EnableMaximize();
    }

    private static void WindowBackdrop_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ExWindow window)
            return;

        window.SetBackdrop(window.WindowSystemBackdrop);
    }
}