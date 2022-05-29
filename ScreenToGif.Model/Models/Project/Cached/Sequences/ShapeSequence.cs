using ScreenToGif.Domain.Enums;
using System.Windows.Shapes;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

public class ShapeSequence : RectSequence
{
    public Shape Shape { get; set; }

    //Maybe don't use built in shape?
    //How to implement new shapes?
    //  Easier to implement via mini-path?

    public ShapeSequence()
    {
        Type = SequenceTypes.Shape;
    }
}