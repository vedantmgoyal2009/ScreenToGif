namespace ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

public abstract class RasterSubSequence : RectSubSequence
{
    /// <summary>
    /// The original width (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalWidth { get; set; }

    /// <summary>
    /// The original height (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalHeight { get; set; }

    public double HorizontalDpi { get; set; }

    public double VerticalDpi { get; set; }

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

    /// <summary>
    /// The number of bytes of the capture content.
    /// </summary>
    public ulong DataLength { get; set; }

    /// <summary>
    /// The position of the stream of pixels (StreamPosition + the size of the headers).
    /// </summary>
    public abstract ulong DataStreamPosition { get; }
}