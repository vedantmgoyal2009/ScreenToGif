using ScreenToGif.Util.Settings;
using System.IO;

namespace ScreenToGif.Util;

public static class StorageHelper
{
    public static void PurgeCache()
    {
        try
        {
            var cache = PathHelper.AdjustPath(UserSettings.All.TemporaryFolderResolved);

            Directory.Delete(cache, true);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Purging cache");
        }
    }
}