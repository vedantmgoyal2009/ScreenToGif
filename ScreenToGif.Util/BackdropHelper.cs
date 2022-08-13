using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Native.External;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScreenToGif.Util;

public static class BackdropHelper
{
    private static int _trueAttribute = 1;
    private static int _falseAttribute = 0;

    public static void SetBackdrop(this Window window, SystemBackdropTypes type)
    {
        var handle = window.GetHandle();

        //Apply fallback color for titlebar.
        if (ThemeHelper.GetActiveTheme() >= AppThemes.Dark)
            ApplyDarkTheme(handle);
        else
            RemoveDarkTheme(handle);

        if (type != SystemBackdropTypes.None)
        {
            window.SetValue(Control.BackgroundProperty, Brushes.Transparent);

            ApplySystemBackdrop(handle, type);
        }
        else
        {
            window.ClearValue(Control.BackgroundProperty);

            RemoveDarkTheme(handle);
            RemoveSystemBackdrop(handle);
        }
    }

    private static void ApplyDarkTheme(IntPtr handle)
    {
        var dwAttribute = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985) ?
            DwmWindowAttributes.UseImmersiveDarkMode : DwmWindowAttributes.UseImmersiveDarkModeBefore20H1;

        DwmApi.DwmSetWindowAttribute(handle, dwAttribute, ref _trueAttribute, Marshal.SizeOf(typeof(int)));
    }

    private static void RemoveDarkTheme(IntPtr handle)
    {
        var dwAttribute = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985) ?
            DwmWindowAttributes.UseImmersiveDarkMode : DwmWindowAttributes.UseImmersiveDarkModeBefore20H1;

        DwmApi.DwmSetWindowAttribute(handle, dwAttribute, ref _falseAttribute, Marshal.SizeOf(typeof(int)));
    }

    private static void ApplySystemBackdrop(IntPtr handle, SystemBackdropTypes type)
    {
        switch (type)
        {
            case SystemBackdropTypes.Auto:
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22523))
                {
                    var attr = (int)SystemBackdropTypes.Auto;

                    DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.SystemBackdropType, ref attr, Marshal.SizeOf(typeof(int)));
                    return;
                }
                
                goto case SystemBackdropTypes.Main;
            }

            case SystemBackdropTypes.Main:
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22523))
                {
                    var attr = (int) SystemBackdropTypes.Main;

                    DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.SystemBackdropType, ref attr, Marshal.SizeOf(typeof(int)));
                    return;
                }

                DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.MicaEffect, ref _trueAttribute, Marshal.SizeOf(typeof(int)));

                break;
            }

            case SystemBackdropTypes.Transient:
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22523))
                {
                    var attr = (int)SystemBackdropTypes.Transient;

                    DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.SystemBackdropType, ref attr, Marshal.SizeOf(typeof(int)));
                    return;
                }

                goto case SystemBackdropTypes.Main;
            }

            case SystemBackdropTypes.Tabbed:
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22523))
                {
                    var attr = (int)SystemBackdropTypes.Tabbed;

                    DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.SystemBackdropType, ref attr, Marshal.SizeOf(typeof(int)));
                    return;
                }

                goto case SystemBackdropTypes.Main;
            }
        }
    }

    private static void RemoveSystemBackdrop(IntPtr handle)
    {
        DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.MicaEffect, ref _falseAttribute, Marshal.SizeOf(typeof(int)));

        var attr = (int)SystemBackdropTypes.None;
        DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttributes.SystemBackdropType, ref attr, Marshal.SizeOf(typeof(int)));
    }
}