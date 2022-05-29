using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

/// <summary>
/// Multiple frame sequence.
/// </summary>
public class FrameSequence : RasterSequence
{
    /// <summary>
    /// Each frame with its timings.
    /// </summary>
    public List<FrameSubSequence> Frames { get; set; } = new();

    public FrameSequence()
    {
        Type = SequenceTypes.Frame;
    }
}