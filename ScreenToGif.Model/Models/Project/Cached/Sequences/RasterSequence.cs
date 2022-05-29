using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

public abstract class RasterSequence : RectSequence
{
    /// <summary>
    /// Origin of the raster frames.
    /// It could be from capture (screen or webcam), media import (gif, apng, image or video) or rasterization of other sequences.
    /// </summary>
    public RasterSequenceSources Origin { get; set; }

    /// <summary>
    /// The original width (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalWidth { get; set; }

    /// <summary>
    /// The original height (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalHeight { get; set; }

    /// <summary>
    /// The number of channels of the images.
    /// 4 is RGBA
    /// 3 is RGB
    /// </summary>
    public byte ChannelCount { get; set; } = 4;

    /// <summary>
    /// The bits per channel in the images.
    /// </summary>
    public byte BitsPerChannel { get; set; } = 8;

    public double HorizontalDpi { get; set; }

    public double VerticalDpi { get; set; }
}