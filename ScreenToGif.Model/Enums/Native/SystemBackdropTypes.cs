namespace ScreenToGif.Domain.Enums.Native;

/// <summary>
/// Flags for specifying the system-drawn backdrop material of a window, including behind the non-client area.
/// https://docs.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type
/// Windows 11 Build 22621
/// </summary>
public enum SystemBackdropTypes
{
    /// <summary>
    /// The default. Let the Desktop Window Manager (DWM) automatically decide the system-drawn backdrop material for this window.
    /// </summary>
    Auto,

    /// <summary>
    /// Don't draw any system backdrop.
    /// </summary>
    None,

    /// <summary>
    /// Draw the backdrop material effect corresponding to a long-lived window.
    /// </summary>
    Main,

    /// <summary>
    /// Draw the backdrop material effect corresponding to a transient window.
    /// </summary>
    Transient,

    /// <summary>
    /// Draw the backdrop material effect corresponding to a window with a tabbed title bar.
    /// </summary>
    Tabbed
}