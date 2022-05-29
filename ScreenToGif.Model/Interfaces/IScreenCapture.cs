using ScreenToGif.Domain.Models.Project.Recording;
using System.Windows.Input;

namespace ScreenToGif.Domain.Interfaces;

public interface IScreenCapture : ICapture
{
    int Left { get; set; }
    int Top { get; set; }
    string DeviceName { get; set; }
    bool IsAcceptingEvents { get; set; }

    /// <summary>
    /// Starts the necessary mechanisms for the capture.
    /// </summary>
    /// <param name="isAutomatic">True if the capture happens based on a clock and not manually or via external triggers.</param>
    /// <param name="delay">The expected delay between frame capture. Well defined for manual capture.</param>
    /// <param name="left">The left offset based on the true left of the virtual screen (all screens combined).</param>
    /// <param name="top">The top offset based on the true top of the virtual screen (all screens combined).</param>
    /// <param name="width">The width of the capture rectangle.</param>
    /// <param name="height">The height of the capture rectangle.</param>
    /// <param name="dpi">The density of pixels of the capture rectangle.</param>
    /// <param name="project">The on memory reference for the capture content.</param>
    void Start(bool isAutomatic, int delay, int left, int top, int width, int height, double dpi, RecordingProject project);

    int CaptureWithCursor(RecordingFrame frame);
    int ManualCapture(RecordingFrame frame, bool showCursor = false);
    Task<int> CaptureWithCursorAsync(RecordingFrame frame);
    Task<int> ManualCaptureAsync(RecordingFrame frame, bool showCursor = false);

    void RegisterCursorEvent(int x, int y, MouseButtonState left, MouseButtonState right, MouseButtonState middle, MouseButtonState firstExtra, MouseButtonState secondExtra, short mouseDelta = 0);
    void RegisterCursorDataEvent(int type, byte[] pixels, int width, int height, int left, int top, int xHotspot, int yHotspot);
    void RegisterKeyEvent(Key key, ModifierKeys modifiers, bool isUppercase, bool wasInjected);
}