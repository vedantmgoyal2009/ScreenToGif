using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public class CursorSubSequenceViewModel : RasterSubSequenceViewModel
{
    private byte _cursorType;
    private ushort _xHotspot;
    private ushort _yHotspot;
    private bool _isLeftButtonDown;
    private bool _isRightButtonDown;
    private bool _isMiddleButtonDown;
    private bool _isFirstExtraButtonDown;
    private bool _isSecondExtraButtonDown;
    private short _mouseWheelDelta;

    /// <summary>
    /// 0 = Unidentified: The cursor type needs to be identified by its pixel content.
    /// 1 = Monochrome: The reference type is a monochrome mouse reference, which is a monochrome bitmap.
    ///     The bitmap's size is specified by width and height in a 1 bits per pixel (bpp) device independent bitmap (DIB) format
    ///     AND mask that is followed by another 1 bpp DIB format XOR mask of the same size.
    /// 2 = Color: The reference type is a color mouse reference, which is a color bitmap.
    ///     The bitmap's size is specified by width and height in a 32 bpp ARGB DIB format.
    /// 4 = Masked Color: The reference type is a masked color mouse reference.
    ///     A masked color mouse reference is a 32 bpp ARGB format bitmap with the mask value in the alpha bits.
    ///     The only allowed mask values are 0 and 0xFF. When the mask value is 0, the RGB value should replace the screen pixel.
    ///     When the mask value is 0xFF, an XOR operation is performed on the RGB value and the screen pixel; the result replaces the screen pixel.
    /// </summary>
    public byte CursorType
    {
        get => _cursorType;
        set => SetProperty(ref _cursorType, value);
    }

    public ushort XHotspot
    {
        get => _xHotspot;
        set => SetProperty(ref _xHotspot, value);
    }

    public ushort YHotspot
    {
        get => _yHotspot;
        set => SetProperty(ref _yHotspot, value);
    }

    public bool IsLeftButtonDown
    {
        get => _isLeftButtonDown;
        set => SetProperty(ref _isLeftButtonDown, value);
    }

    public bool IsRightButtonDown
    {
        get => _isRightButtonDown;
        set => SetProperty(ref _isRightButtonDown, value);
    }

    public bool IsMiddleButtonDown
    {
        get => _isMiddleButtonDown;
        set => SetProperty(ref _isMiddleButtonDown, value);
    }

    public bool IsFirstExtraButtonDown
    {
        get => _isFirstExtraButtonDown;
        set => SetProperty(ref _isFirstExtraButtonDown, value);
    }

    public bool IsSecondExtraButtonDown
    {
        get => _isSecondExtraButtonDown;
        set => SetProperty(ref _isSecondExtraButtonDown, value);
    }

    public short MouseWheelDelta
    {
        get => _mouseWheelDelta;
        set => SetProperty(ref _mouseWheelDelta, value);
    }
    
    public bool IsMiddleScrollUp => MouseWheelDelta > 0;

    public bool IsMiddleScrollDown => MouseWheelDelta < 0;

    public bool IsMiddleScroll => IsMiddleScrollUp || IsMiddleScrollDown;

    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 59 bytes.
    /// </summary>
    public override ulong DataStreamPosition => StreamPosition + 59;

    public static CursorSubSequenceViewModel FromModel(CursorSubSequence sequence)
    {
        return new CursorSubSequenceViewModel
        {
            Type = sequence.Type,
            TimeStampInTicks = sequence.TimeStampInTicks,
            StreamPosition = sequence.StreamPosition,
            Left = sequence.Left,
            Top = sequence.Top,
            Width = sequence.Width,
            Height = sequence.Height,
            Angle = sequence.Angle,
            OriginalWidth = sequence.OriginalWidth,
            OriginalHeight = sequence.OriginalHeight,
            HorizontalDpi = sequence.HorizontalDpi,
            VerticalDpi = sequence.VerticalDpi,
            ChannelCount = sequence.ChannelCount,
            BitsPerChannel = sequence.BitsPerChannel,
            DataLength = sequence.DataLength,
            CursorType = sequence.CursorType,
            XHotspot = sequence.XHotspot,
            YHotspot = sequence.YHotspot,
            IsLeftButtonDown = sequence.IsLeftButtonDown,
            IsRightButtonDown = sequence.IsRightButtonDown,
            IsMiddleButtonDown = sequence.IsMiddleButtonDown,
            IsFirstExtraButtonDown = sequence.IsFirstExtraButtonDown,
            IsSecondExtraButtonDown = sequence.IsSecondExtraButtonDown
        };
    }
}