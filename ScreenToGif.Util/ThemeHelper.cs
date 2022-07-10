using Microsoft.Win32;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util.Settings;
using System.Windows;

namespace ScreenToGif.Util;

public static class ThemeHelper
{
    public static void SelectTheme(AppThemes theme = AppThemes.Light)
    {
        if (theme == AppThemes.FollowSystem)
            theme = IsSystemUsingDarkTheme() ? AppThemes.Dark : AppThemes.Light;

        //Checks if the theme is already the current in use.
        var last = Application.Current.Resources.MergedDictionaries.LastOrDefault(l => l.Source != null && l.Source.ToString().Contains("Colors/"));

        if (last?.Source.ToString().EndsWith($"/{theme}.xaml") == true)
            return;

        //Tries to switch to the new theme.
        var res = Application.Current.Resources.MergedDictionaries.FirstOrDefault(f => f.Source != null && f.Source.ToString().EndsWith($"Colors/{theme}.xaml"));

        if (res == null)
        {
            res = Application.Current.Resources.MergedDictionaries.FirstOrDefault(f => f.Source != null && f.Source.ToString().EndsWith("Colors/Light.xaml"));
            UserSettings.All.MainTheme = AppThemes.Light;
        }

        Application.Current.Resources.MergedDictionaries.Remove(res);
        Application.Current.Resources.MergedDictionaries.Add(res);

        //Forces the refresh of the vectors with dynamic resources inside.
        var glyphs = Application.Current.Resources.MergedDictionaries.FirstOrDefault(f => f.Source != null && f.Source.ToString().EndsWith("Resources/Glyphs.xaml"));

        Application.Current.Resources.MergedDictionaries.Remove(glyphs);
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("Resources/Glyphs.xaml", System.UriKind.RelativeOrAbsolute) });

        //TODO: Update the backdrop for all opened windows.
        //TODO: Update the theme for the notification icon.

        RefreshNotificationIcon();
    }

    public static void SelectGridTheme()
    {
        if (!UserSettings.All.GridColorsFollowSystem)
            return;

        var isSystemUsingDark = IsSystemUsingDarkTheme();

        UserSettings.All.GridColor1 = isSystemUsingDark ? Constants.DarkEven : Constants.VeryLightEven;
        UserSettings.All.GridColor2 = isSystemUsingDark ? Constants.DarkOdd : Constants.VeryLightOdd;
    }

    public static bool IsSystemUsingDarkTheme()
    {
        try
        {
            using var sub = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            if (sub?.GetValue("AppsUseLightTheme") is int key)
                return key == 0;
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Not possible to get system's theme setting.");
        }

        return false;
    }

    public static AppThemes GetActiveTheme()
    {
        var theme = UserSettings.All.MainTheme;

        if (theme == AppThemes.FollowSystem)
            theme = IsSystemUsingDarkTheme() ? AppThemes.Dark : AppThemes.Light;

        return theme;
    }

    private static void RefreshNotificationIcon()
    {
        //if (App.NotifyIcon == null)
        //    return;

        //Maybe store current locale in AppViewModel and trigger change via command.

        //App.NotifyIcon.RefreshVisual();
    }
}