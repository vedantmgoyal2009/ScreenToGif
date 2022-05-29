using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

/// <summary>
/// Holds cursor related events (cursor changes, clicks, movements, etc.).
/// </summary>
public class CursorSequence : RectSequence
{
    /// <summary>
    /// Each cursor with its timings.
    /// </summary>
    public List<CursorSubSequence> CursorEvents { get; set; } = new();
    
    public CursorSequence()
    {
        Type = SequenceTypes.Cursor;
    }
}