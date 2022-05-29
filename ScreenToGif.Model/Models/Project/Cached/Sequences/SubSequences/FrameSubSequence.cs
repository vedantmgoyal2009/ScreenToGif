using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

public class FrameSubSequence : RasterSubSequence
{
    /// <summary>
    /// Frame delay in milliseconds.
    /// </summary>
    public long Delay { get; set; }

    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 55 bytes.
    /// </summary>
    public override ulong DataStreamPosition => StreamPosition + 55;

    public FrameSubSequence()
    {
        Type = SubSequenceTypes.Frame;
    }
}