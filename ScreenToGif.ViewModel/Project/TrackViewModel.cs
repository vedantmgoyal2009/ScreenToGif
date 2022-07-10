using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Extensions;
using ScreenToGif.ViewModel.Editor;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.ViewModel.Project;

public class TrackViewModel : BaseViewModel, ITrack
{
    private int _id = 0;
    private bool _isVisible = true;
    private bool _isLocked = false;
    private string _name = "";
    private Brush _accent = Brushes.Transparent;
    private string _cachePath = "";
    private readonly EditorViewModel _editorViewModel = null;
    private ObservableCollection<ISequence> _sequences = new();

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            SetProperty(ref _isVisible, value);

            EditorViewModel?.Render();
        }
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

    internal EditorViewModel EditorViewModel
    {
        get => _editorViewModel;
        private init => SetProperty(ref _editorViewModel, value);
    }

    /// <summary>
    /// A track can have multiple sequences of the same type.
    /// </summary>
    public ObservableCollection<ISequence> Sequences
    {
        get => _sequences;
        set => SetProperty(ref _sequences, value);
    }

    public static TrackViewModel FromModel(Track track, EditorViewModel editorViewModel)
    {
        return new TrackViewModel
        {
            Id = track.Id,
            IsVisible = track.IsVisible,
            IsLocked = track.IsLocked,
            Name = track.Name,
            Accent = new SolidColorBrush(ColorExtensions.GenerateRandomPastel()),
            CachePath = track.CachePath,
            EditorViewModel = editorViewModel,
            Sequences = new ObservableCollection<ISequence>(track.Sequences.Select(s => SequenceViewModel.FromModel(s, editorViewModel)))
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