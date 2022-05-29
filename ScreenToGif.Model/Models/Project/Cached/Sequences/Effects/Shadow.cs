using ScreenToGif.Domain.Enums;
using System.Windows.Media;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences.Effects;

public class Shadow : Effect
{
    public Color Color { get;set; } 
        
    public double Direction { get; set; }
        
    public double BlurRadius { get; set; }

    public double Opacity { get; set; }

    public double Depth { get; set; }

    public Shadow()
    {
        Type = EffectTypes.Shadow;
    }
}