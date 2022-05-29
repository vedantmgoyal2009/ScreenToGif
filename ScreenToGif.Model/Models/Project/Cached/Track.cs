namespace ScreenToGif.Domain.Models.Project.Cached;

public class Track
{
    /// <summary>
    /// Unique, static and sequential Id for the track.
    /// With this Id, it's possible to have access to the correct cache files.
    /// </summary>
    public ushort Id { get; set; }

    /// <summary>
    /// True if the track contents should be rendered and displayed in the final image.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Means that the track is locked and cannot be manipulated by the user.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Visible name of the track in the timeline.
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// A track can have multiple sequences of the same type.
    /// </summary>
    public List<Sequence> Sequences { get; set; } = new();

    /// <summary>
    /// A binary cache containing a simple structure with the details of the track.
    /// </summary>
    public string CachePath { get; set; }
}