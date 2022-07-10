using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;

namespace ScreenToGif.Util;

public static class IconHelper
{
    /// <summary>
    /// Reads a given image resource into a WinForms icon.
    /// </summary>
    /// <param name="imageSource">Image source pointing to an icon file (*.ico).</param>
    /// <returns>An icon object that can be used with the taskbar area.</returns>
    public static Icon ToIcon(this ImageSource imageSource)
    {
        if (imageSource == null)
            return null;

        StreamResourceInfo streamInfo = null;

        try
        {
            var uri = new Uri(imageSource.ToString());
            streamInfo = Application.GetResourceStream(uri);

            if (streamInfo == null)
                throw new ArgumentException($"It was not possible to load the image source: '{imageSource}'.");

            return new Icon(streamInfo.Stream);
        }
        catch (Win32Exception e)
        {
            LogWriter.Log(e, "It was not possible to load the notification area icon.", $"StreamInfo is null? {streamInfo == null}, Native error code: {e.NativeErrorCode}");
            return null;
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "It was not possible to load the notification area icon.", $"StreamInfo is null? {streamInfo == null}");
            return null;
        }
        finally
        {
            streamInfo?.Stream?.Dispose();
        }
    }
}