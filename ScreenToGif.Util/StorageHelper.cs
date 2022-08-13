using ScreenToGif.Util.Settings;
using System.IO;

namespace ScreenToGif.Util;

public static class StorageHelper
{
    public static void PurgeCache()
    {
        try
        {
            var cache = Path.Combine(PathHelper.AdjustPath(UserSettings.All.TemporaryFolderResolved), "ScreenToGif", "Recording");

            Directory.Delete(cache, true);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Purging cache");
        }
    }

    public static void PurgeCache(int days)
    {
        try
        {
            var cache = Path.Combine(PathHelper.AdjustPath(UserSettings.All.TemporaryFolderResolved), "ScreenToGif", "Recording");

            if (!Directory.Exists(cache))
                return;

            var list = Directory.GetDirectories(cache).Select(x => new DirectoryInfo(x)).Where(w => (DateTime.Now - w.CreationTime).TotalDays > (days > 0 ? days : 5)).ToList();

            foreach (var folder in list)
            {
                if (MutexList.IsInUse(folder.Name))
                    continue;

                Directory.Delete(folder.FullName, true);
            }
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Purging cache (with configurable date span)");
        }
    }

    //RecordingProject
    //CachedProject
    //Updates
}