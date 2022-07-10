namespace ScreenToGif.Domain.Interfaces;

public interface ISequence
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string CachePath { get; set; }

    public void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality, string cachePath);
}