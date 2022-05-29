namespace ScreenToGif.Domain.Structs;

/// <summary>
/// Describes a color in terms of Hue, Saturation, and Value (brightness)
/// </summary>
public struct HsvColor
{
    public double H;
    public double S;
    public double V;

    public HsvColor(double h, double s, double v)
    {
        H = h;
        S = s;
        V = v;
    }
}