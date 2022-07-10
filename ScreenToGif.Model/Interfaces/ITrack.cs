using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.Domain.Interfaces;

public interface ITrack
{
    public int Id { get; set; }

    public bool IsVisible { get; set; }

    public bool IsLocked { get; set; }

    public string Name { get; set; }

    public Brush Accent { get; set; }

    public ObservableCollection<ISequence> Sequences { get; set; }
}