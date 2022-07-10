using System.Net;

namespace ScreenToGif.Util;

public static class SecurityHelper
{
    public static void SetSecurityProtocol()
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to set the network properties");
        }
    }
}