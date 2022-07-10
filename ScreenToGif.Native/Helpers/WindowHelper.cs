using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Native.External;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ScreenToGif.Native.Helpers;

public static class WindowHelper
{
    private const int GetWindowsLongStyle = -16;
    private const int GetWindowsLongExStyle = -20;

    public static IntPtr GetWindowPtr(this Window window)
    {
        return new WindowInteropHelper(window).Handle;
    }

    public static void DisableMaximize(this Window window)
    {
        var ptr = GetWindowPtr(window);

        User32.SetWindowLong(ptr, GetWindowsLongStyle, User32.GetWindowLong(ptr, GetWindowsLongStyle) &~ (int)WindowStyles.Maximizebox);
    }

    public static void DisableMinimize(this Window window)
    {
        var ptr = GetWindowPtr(window);

        User32.SetWindowLong(ptr, GetWindowsLongStyle, User32.GetWindowLong(ptr, GetWindowsLongStyle) & ~(int)WindowStyles.Minimizebox);
    }

    public static void EnableMaximize(this Window window)
    {
        var ptr = GetWindowPtr(window);

        User32.SetWindowLong(ptr, GetWindowsLongStyle, User32.GetWindowLong(ptr, GetWindowsLongStyle) & (int)WindowStyles.Maximizebox);
    }

    public static void EnableMinimize(this Window window)
    {
        var ptr = GetWindowPtr(window);

        User32.SetWindowLong(ptr, GetWindowsLongStyle, User32.GetWindowLong(ptr, GetWindowsLongStyle) & (int)WindowStyles.Minimizebox);
    }

    public static void SetCornerPreference(this Window window, CornerPreferences preference)
    {
        var ptr = GetWindowPtr(window);

        var attr = (int)preference;

        DwmApi.DwmSetWindowAttribute(ptr, DwmWindowAttributes.WindowCornerPreference, ref attr, Marshal.SizeOf(typeof(int)));
    }

    public static IntPtr NearestMonitorForWindow(IntPtr window)
    {
        return User32.MonitorFromWindow(window, Constants.MonitorDefaultToNearest);
    }
}