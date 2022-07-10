namespace ScreenToGif.Util.Settings;

public partial class UserSettings
{
    #region Options • Application

    private const bool SingleInstanceOriginal = true;
    public bool SingleInstance
    {
        get => (bool)GetValue(SingleInstanceOriginal);
        set => SetValue(value, SingleInstanceOriginal);
    }

    private const bool StartMinimizedOriginal = false;
    public bool StartMinimized
    {
        get => (bool)GetValue(StartMinimizedOriginal);
        set => SetValue(value, StartMinimizedOriginal);
    }

    //TODO: Transform into an enum.
    private const int StartUpOriginal = 0;
    /// <summary>
    /// The homepage of the app:
    /// 0 - Startup window.
    /// 1 - Recorder window.
    /// 2 - Webcam window.
    /// 3 - Board window.
    /// 4 - Editor window.
    /// </summary>
    public int StartUp
    {
        get => (int)GetValue(StartUpOriginal);
        set => SetValue(value, StartUpOriginal);
    }

    #endregion
}