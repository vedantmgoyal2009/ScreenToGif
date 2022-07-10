using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using ScreenToGif.ViewModel.Editor;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public class FrameSubSequenceViewModel : RasterSubSequenceViewModel
{
    private long _delay;
    
    /// <summary>
    /// Frame delay in milliseconds.
    /// </summary>
    public long Delay
    {
        get => _delay;
        set => SetProperty(ref _delay, value);
    }

    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 55 bytes.
    /// </summary>
    public override ulong DataStreamPosition => StreamPosition + 55;

    public static FrameSubSequenceViewModel FromModel(FrameSubSequence sequence, EditorViewModel baseViewModel)
    {
        return new FrameSubSequenceViewModel
        {
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
            Delay = sequence.Delay
        };
    }
}