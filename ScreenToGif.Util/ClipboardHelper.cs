using System.Runtime.InteropServices;
using System.Windows;

namespace ScreenToGif.Util;

public static class ClipboardHelper
{
    public static async Task CopyToClipboard(this string text)
    {
        for (var i = 0; i < 10; i++)
        {
            try
            {
                Clipboard.SetDataObject(text, true);
                break;
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode != 0x800401D0) //CLIPBRD_E_CANT_OPEN
                    throw;
            }

            await Task.Delay(100);
        }
    }
}