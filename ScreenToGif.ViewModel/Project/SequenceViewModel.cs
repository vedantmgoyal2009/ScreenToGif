using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.ViewModel.Editor;
using ScreenToGif.ViewModel.Project.Sequences;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.ViewModel.Project;

public abstract class SequenceViewModel : BaseViewModel, ISequence
{
    private int _id = 0;
    private SequenceTypes _type = SequenceTypes.Unknown;
    private TimeSpan _startTime = TimeSpan.Zero;
    private TimeSpan _endTime = TimeSpan.Zero;
    private double _opacity = 1;
    private Brush _background = null;
    private ObservableCollection<object> _effects = new();
    private ulong _streamPosition = 0;
    private string _cachePath = "";
    private EditorViewModel _editorViewModel = null;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public SequenceTypes Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            SetProperty(ref _startTime, value);

            EditorViewModel?.Render();
        }
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            SetProperty(ref _endTime, value);

            EditorViewModel?.Render();
        }
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            SetProperty(ref _opacity, value);

            EditorViewModel?.Render();
        }
    }

    public Brush Background
    {
        get => _background;
        set
        {
            SetProperty(ref _background, value);

            EditorViewModel?.Render();
        }
    }

    public ObservableCollection<object> Effects
    {
        get => _effects;
        set => SetProperty(ref _effects, value);
    }

    public ulong StreamPosition
    {
        get => _streamPosition;
        set => SetProperty(ref _streamPosition, value);
    }

    public string CachePath
    {
        get => _cachePath;
        set => SetProperty(ref _cachePath, value);
    }

    internal EditorViewModel EditorViewModel
    {
        get => _editorViewModel;
        set => SetProperty(ref _editorViewModel, value);
    }

    public static SequenceViewModel FromModel(Sequence sequence, EditorViewModel baseViewModel)
    {
        switch (sequence)
        {
            case FrameSequence raster:
                return FrameSequenceViewModel.FromModel(raster, baseViewModel);

            case CursorSequence cursor:
                return CursorSequenceViewModel.FromModel(cursor, baseViewModel);

            //case KeySequence key:
            //    return KeySequenceViewModel.FromModel(key, baseViewModel);

            //TODO: Copy all data.
        }

        return null;
    }

    public abstract void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality, string cachePath);
}