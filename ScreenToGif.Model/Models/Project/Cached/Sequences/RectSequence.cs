namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

/// <summary>
/// Primitive sequence object which has a defined sizing information.
/// </summary>
public abstract class RectSequence : Sequence
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