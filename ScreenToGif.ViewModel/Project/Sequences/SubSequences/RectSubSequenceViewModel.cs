namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public abstract class RectSubSequenceViewModel : SubSequenceViewModel
{
    private int _left;
    private int _top;
    private ushort _width;
    private ushort _height;
    private double _angle;

    public int Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }

    public int Top
    {
        get => _top;
        set => SetProperty(ref _top, value);
    }

    public ushort Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public ushort Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    public double Angle
    {
        get => _angle;
        set => SetProperty(ref _angle, value);
    }
}