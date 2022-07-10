using ScreenToGif.Domain.Models;

namespace ScreenToGif.Settings.Migrations;

internal class Migration2_37_0To3_0_0
{
    internal static bool Up(List<Property> properties)
    {
        var startup = properties.FirstOrDefault(a => a.Key == "StartUp");

        if (startup?.Value == "5")
        {
            startup.Value = "0";

            var startMinimized = properties.FirstOrDefault(a => a.Key == "StartMinimized");
            startMinimized.Value = "true";

            var showNotificationIcon = properties.FirstOrDefault(a => a.Key == "ShowNotificationIcon");
            showNotificationIcon.Value = "true";
        }


        //Update namespaces.
        //Remove settings.

        //Remove UserSettings.All.EditorExtendChrome;

        return true;
    }
}