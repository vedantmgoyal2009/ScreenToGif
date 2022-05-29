namespace ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

/// <summary>
/// A sub-sequence that can be moved and resize.
/// </summary>
public abstract class RectSubSequence : SubSequence
{
    public int Left { get; set; }

    public int Top { get; set; }

    public ushort Width { get; set; }

    public ushort Height { get; set; }

    /// <summary>
    /// Angle in degrees.
    /// </summary>
    public double Angle { get; set; }
}