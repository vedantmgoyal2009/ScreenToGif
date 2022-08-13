namespace ScreenToGif.Util.Extensions;

public static class VersionExtensions
{
    public static string ToStringShort(this Version version)
    {
        var result = $"{version.Major}";

        if (version.Minor > 0)
            result += ".{ version.Minor}";

        if (version.Build > 0)
            result += $".{version.Build}";

        if (version.Revision > 0)
            result += $".{version.Revision}";

        return result;
    }
}