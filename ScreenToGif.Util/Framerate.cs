using System.Diagnostics;
using ScreenToGif.Util.Settings;

namespace ScreenToGif.Util;

/// <summary>
/// Frame rate monitor. 
/// </summary>
public class CaptureStopwatch
{
    #region Variables

    private Stopwatch _stopwatch;

    private int _interval = 15;
    private long _intervalInTicks = 150000;
    private bool _started = true;
    private bool _fixedRate;

    #endregion

    public bool IsRunning => _stopwatch?.IsRunning ?? false;

    /// <summary>
    /// Prepares the FrameRate monitor.
    /// </summary>
    /// <param name="interval">The selected interval of each snapshot.</param>
    public void Initialize(int interval)
    {
        _stopwatch = new Stopwatch();

        _interval = interval;
        _intervalInTicks = TimeSpan.FromMilliseconds(interval).Ticks;

        _fixedRate = UserSettings.All.FixedFrameRate;
    }

    /// <summary>
    /// Prapares the framerate monitor
    /// </summary>
    /// <param name="useFixed">If true, uses the fixed internal provided.</param>
    /// <param name="interval">The fixed interval to be used.</param>
    public void Initialize(bool useFixed, int interval)
    {
        _stopwatch = new Stopwatch();

        _interval = interval;
        _fixedRate = useFixed;
    }

    /// <summary>
    /// Gets the diff between the last call.
    /// </summary>
    /// <returns>The amount of seconds.</returns>
    [Obsolete]
    public int GetMilliseconds()
    {
        if (_fixedRate)
            return _interval;

        if (_started)
        {
            _started = false;
            _stopwatch.Start();
            return _interval;
        }

        var mili = (int)_stopwatch.ElapsedMilliseconds;
        _stopwatch.Restart();

        return mili;
    }

    /// <summary>
    /// Gets the elapsed ticks since the start of the stopwatch.
    /// Returns a fixed value for non automatic capture.
    /// </summary>
    public long GetElapsedTicks()
    {
        if (_fixedRate)
            return _intervalInTicks;

        if (_stopwatch.IsRunning)
            return _stopwatch?.ElapsedTicks ?? -1L;

        //The first frame captured needs to start the stopwatch.
        _stopwatch.Start();

        return 0;
    }

    /// <summary>
    /// Stops/pauses the stopwatch.
    /// </summary>
    public void Stop()
    {
        _stopwatch.Stop();
    }
}