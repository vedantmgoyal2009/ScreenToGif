namespace ScreenToGif.Domain.Enums;

public enum SequenceTypes : int
{
    Unknown = 0,

    /// <summary>
    /// A sequence that contains a single brush data.
    /// </summary>
    Brush = 1,

    /// <summary>
    /// A sequence that holds frames.
    /// </summary>
    Frame,

    /// <summary>
    /// A sequence that holds all cursor events.
    /// </summary>
    Cursor,

    /// <summary>
    /// A sequence that holds all keys events.
    /// </summary>
    Key,

    /// <summary>
    /// A sequence that holds text data.
    /// </summary>
    Text,

    /// <summary>
    /// A sequence that holds a shape.
    /// </summary>
    Shape,

    /// <summary>
    /// A sequence that holds strokes (drawings).
    /// </summary>
    Drawing,

    /// <summary>
    /// A sequecne that holds progress related information.
    /// </summary>
    Progress,
    
    /// <summary>
    /// A sequence that holds obfuscation spots.
    /// </summary>
    Obfuscation,

    /// <summary>
    /// A sequence that holds a single image, non-capture related.
    /// </summary>
    Image,

    TitleFrame, //Maybe it should be a layer type of frame?
    Cinemagraph //How to implement?
}