using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public abstract class SubSequenceViewModel : BindableBase
{
    private SubSequenceTypes _type;
    private ulong _timeStampInTicks;
    private ulong _streamPosition;

    public SubSequenceTypes Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    /// <summary>
    /// Ticks since the start of the sequence.
    /// </summary>
    public ulong TimeStampInTicks
    {
        get => _timeStampInTicks;
        set => SetProperty(ref _timeStampInTicks, value);
    }

    /// <summary>
    /// Position of this sub-sequence in the stream.
    /// </summary>
    public ulong StreamPosition
    {
        get => _streamPosition;
        set => SetProperty(ref _streamPosition, value);
    }
}