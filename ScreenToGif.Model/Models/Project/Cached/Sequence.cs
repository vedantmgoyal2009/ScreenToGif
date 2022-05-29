using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.Effects;
using System.Windows.Media;

namespace ScreenToGif.Domain.Models.Project.Cached;

/// <summary>
/// Base sequence class, used by all sequence concrete types.
/// A sequence is a track event, that has a start and end.
/// These track events can be, for example, capture frames, shapes, cursor events, etc.
/// </summary>
public abstract class Sequence
{
    /// <summary>
    /// Unique, static and sequential Id for the sequence.
    /// With this Id, it's possible to have access to the correct cache files.
    /// </summary>
    public ushort Id { get; set; }

    /// <summary>
    /// Type of sequence.
    /// Each sequence can only be of one type, such as frames, cursor events, etc.
    /// </summary>
    public SequenceTypes Type { get; set; }

    /// <summary>
    /// The start time in relation to the sequence's track.
    /// Sequences can be moved back and forth within a track timeline.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// The end time in relation to the sequence's start and content duration.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// The opacity of the sequence, with 1 being fully opaque and 0 being invisible.
    /// </summary>
    public double Opacity { get; set; } = 1;

    /// <summary>
    /// The background brush of the sequence.
    /// This brush will be rendered below the sequence actual content.
    /// </summary>
    public Brush Background { get; set; }

    /// <summary>
    /// A list of effects that can be applied to the entire sequence.
    /// Such as shadow, color filters, etc.
    /// </summary>
    public List<Shadow> Effects { get; set; } = new();

    /// <summary>
    /// The position of the stream in the cache file.
    /// It's the stream position before the first parameter of this class (Id) was written to the cache.
    /// </summary>
    public ulong StreamPosition { get; set; }

    /// <summary>
    /// A binary cache containing a simple structure with the details of the sequence.
    /// </summary>
    public string CachePath { get; set; }
}