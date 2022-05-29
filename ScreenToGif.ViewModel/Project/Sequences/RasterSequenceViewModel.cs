using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Project.Sequences;

public abstract class RasterSequenceViewModel : RectSequenceViewModel
{
    private RasterSequenceSources _origin;
    private byte _channelCount = 4;
    private byte _bitsPerChannel = 8;
    private ushort _originalWidth;
    private ushort _originalHeight;
    private double _horizontalDpi;
    private double _verticalDpi;

    /// <summary>
    /// Origin of the raster frames.
    /// It could be from capture (screen or webcam), media import (gif, apng, image or video) or rasterization of other sequences.
    /// </summary>
    public RasterSequenceSources Origin
    {
        get => _origin;
        set => SetProperty(ref _origin, value);
    }

    /// <summary>
    /// The original width (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalWidth
    {
        get => _originalWidth;
        set => SetProperty(ref _originalWidth, value);
    }

    /// <summary>
    /// The original height (pre-resize) of the raster image.
    /// </summary>
    public ushort OriginalHeight
    {
        get => _originalHeight;
        set => SetProperty(ref _originalHeight, value);
    }

    /// <summary>
    /// The number of channels of the images.
    /// 4 is RGBA
    /// 3 is RGB
    /// </summary>
    public byte ChannelCount
    {
        get => _channelCount;
        set => SetProperty(ref _channelCount, value);
    }

    /// <summary>
    /// The bits per channel in the images.
    /// </summary>
    public byte BitsPerChannel
    {
        get => _bitsPerChannel;
        set => SetProperty(ref _bitsPerChannel, value);
    }

    public double HorizontalDpi
    {
        get => _horizontalDpi;
        set => SetProperty(ref _horizontalDpi, value);
    }

    public double VerticalDpi
    {
        get => _verticalDpi;
        set => SetProperty(ref _verticalDpi, value);
    }
}