using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.ViewModel.Project;

public class TrackViewModel : BaseViewModel
{
    private int _id = 0;
    private bool _isVisible = true;
    private bool _isLocked = false;
    private string _name = "";
    private Brush _accent = Brushes.Transparent;
    private string _cachePath = "";
    private ObservableCollection<SequenceViewModel> _sequences = new();

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public Brush Accent
    {
        get => _accent;
        set => SetProperty(ref _accent, value);
    }

    public string CachePath
    {
        get => _cachePath;
        set => SetProperty(ref _cachePath, value);
    }

    /// <summary>
    /// A track can have multiple sequences of the same type.
    /// </summary>
    public ObservableCollection<SequenceViewModel> Sequences
    {
        get => _sequences;
        set => SetProperty(ref _sequences, value);
    }

    public static TrackViewModel FromModel(Track track)
    {
        return new TrackViewModel
        {
            Id = track.Id,
            IsVisible = track.IsVisible,
            IsLocked = track.IsLocked,
            Name = track.Name,
            Accent = new SolidColorBrush(ColorExtensions.GenerateRandomPastel()),
            CachePath = track.CachePath,
            Sequences = new ObservableCollection<SequenceViewModel>(track.Sequences.Select(SequenceViewModel.FromModel))
        };
    }

    public void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality)
    {
        if (!IsVisible)
            return;

        foreach (var sequence in Sequences)
            sequence.RenderAt(current, canvasWidth, canvasHeight, timestamp, quality, sequence.CachePath);
    }
}