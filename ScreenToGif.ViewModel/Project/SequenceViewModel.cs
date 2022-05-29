using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.ViewModel.Project.Sequences;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.ViewModel.Project;

public abstract class SequenceViewModel : BaseViewModel
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
        set => SetProperty(ref _startTime, value);
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public Brush Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
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

    public static SequenceViewModel FromModel(Sequence sequence)
    {
        switch (sequence)
        {
            case FrameSequence raster:
                return FrameSequenceViewModel.FromModel(raster);

            case CursorSequence cursor:
                return CursorSequenceViewModel.FromModel(cursor);

            //case KeySequence key:
            //    return KeySequenceViewModel.FromModel(key);
            
            //TODO: Copy all data.
        }

        return null;
    }

    internal abstract void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality, string cachePath);
}