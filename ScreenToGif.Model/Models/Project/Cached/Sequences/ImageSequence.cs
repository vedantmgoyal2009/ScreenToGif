using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

/// <summary>
/// Single image sequence.
/// </summary>
public class ImageSequence : RasterSequence
{
    /// <summary>
    /// The path to the source image.
    /// </summary>
    public string SourcePath { get; set; }

    /// <summary>
    /// The number of bytes of the image.
    /// </summary>
    public ulong DataLength { get; set; }

    public ImageSequence()
    {
        Type = SequenceTypes.Image;
    }
}