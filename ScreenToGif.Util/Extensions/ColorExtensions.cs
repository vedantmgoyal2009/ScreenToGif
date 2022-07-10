using ScreenToGif.Domain.Structs;
using System.Windows.Media;

namespace ScreenToGif.Util.Extensions;

public static class ColorExtensions
{
    #region Information

    public static int GetBrightness4(this Color color)
    {
        return color.R + (2 * color.G) + color.B;
    }

    public static float GetBrightness3(this Color color)
    {
        var num = color.R / 255f;
        var num2 = color.G / 255f;
        var num3 = color.B / 255f;
        var num4 = num;
        var num5 = num;
        if (num2 > num4)
            num4 = num2;
        if (num3 > num4)
            num4 = num3;
        if (num2 < num5)
            num5 = num2;
        if (num3 < num5)
            num5 = num3;
        return (num4 + num5) / 2f;
    }

    public static double GetBrightness2(this Color c)
    {
        return 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
        //return (0.299*c.R + 0.587*c.G + 0.114*c.B);
    }

    public static int GetBrightness1(this Color c)
    {
        return (int)Math.Sqrt(
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
    }

    public static int GetBrightness(this Color c)
    {
        return (2 * c.R) + (5 * c.G) + c.B;
    }

    public static float GetHue(this Color color)
    {
        if (color.R == color.G && color.G == color.B)
            return 0f;

        var num = color.R / 255f;
        var num2 = color.G / 255f;
        var num3 = color.B / 255f;
        var num7 = 0f;
        var num4 = num;
        var num5 = num;

        if (num2 > num4)
            num4 = num2;
        if (num3 > num4)
            num4 = num3;
        if (num2 < num5)
            num5 = num2;
        if (num3 < num5)
            num5 = num3;

        var num6 = num4 - num5;

        if (num == num4)
            num7 = (num2 - num3) / num6;
        else if (num2 == num4)
            num7 = 2f + (num3 - num) / num6;
        else if (num3 == num4)
            num7 = 4f + (num - num2) / num6;

        num7 *= 60f;

        if (num7 < 0f)
            num7 += 360f;

        return num7;
    }

    public static float GetSaturation(this Color color)
    {
        var num = color.R / 255f;
        var num2 = color.G / 255f;
        var num3 = color.B / 255f;

        var num7 = 0f;
        var num4 = num;
        var num5 = num;
        if (num2 > num4)
            num4 = num2;
        if (num3 > num4)
            num4 = num3;
        if (num2 < num5)
            num5 = num2;
        if (num3 < num5)
            num5 = num3;
        if (num4 == num5)
            return num7;

        var num6 = (num4 + num5) / 2f;

        if (num6 <= 0.5)
            return (num4 - num5) / (num4 + num5);

        return (num4 - num5) / (2f - num4 - num5);
    }

    /// <summary>
    /// Color brightness as perceived.
    /// </summary>
    /// <param name="c">The Color</param>
    /// <returns>The brightness.</returns>
    public static float GetLuminance(this Color c)
    {
        return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f;
    }

    #endregion

    #region Comparisons

    /// <summary>
    /// Closest match for hues only.
    /// </summary>
    public static int ClosestColorHue(List<Color> colors, Color target)
    {
        var hue1 = target.GetHue();
        var diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(n => n);

        return diffs.ToList().FindIndex(n => n == diffMin);
    }

    /// <summary>
    /// Closest match in RGB space.
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int ClosestColorRgb(List<Color> colors, Color target)
    {
        //var colorDiffs = colors.AsParallel().Select(n => ColorDiff(n, target)).Min(n => n);
        //return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);

        var distance = int.MaxValue;
        var indexOfMin = -1;

        Parallel.For(0, colors.Count, (i, x) =>
        {
            var diff = ColorDiff(colors[i], target);

            if (diff < distance)
            {
                distance = diff;
                indexOfMin = i;
            }

            if (distance == 0)
                x.Break();
        });

        return indexOfMin;

        //return colors.AsParallel().Select(n=> ColorDiff(n, target)).IndexOfMin();
    }

    /// <summary>
    /// Weighed distance using hue, saturation and brightness.
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int ClosestColorHsb(List<Color> colors, Color target)
    {
        float ColorNum(Color c)
        {
            var factorSat = 3;
            var factorBri = 3;

            return c.GetSaturation() * factorSat + GetBrightness(c) * factorBri;
        }

        var hue1 = target.GetHue();
        var num1 = ColorNum(target);
        var diffs = colors.Select(n => Math.Abs(ColorNum(n) - num1) + GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(x => x);

        return diffs.ToList().FindIndex(n => n == diffMin);
    }

    /// <summary>
    /// Gets the distance between two hues.
    /// </summary>
    /// <param name="hue1">Hue 1</param>
    /// <param name="hue2">Hue 2</param>
    /// <returns>The distance.</returns>
    public static float GetHueDistance(float hue1, float hue2)
    {
        var d = Math.Abs(hue1 - hue2);
        return d > 180 ? 360 - d : d;
    }
    
    /// <summary>
    /// Gets the distance in the RGB space.
    /// </summary>
    /// <param name="first">Color 1</param>
    /// <param name="second">Color 2</param>
    /// <returns>The distance.</returns>
    public static int ColorDiff(Color first, Color second)
    {
        return (int)Math.Sqrt((first.R - second.R) * (first.R - second.R) + (first.G - second.G) * (first.G - second.G) + (first.B - second.B) * (first.B - second.B));
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Converts an RGB color to an HSV color
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="b">Blue</param>
    /// <param name="g">Green</param>
    /// <returns>A HsvColor object.</returns>
    public static HsvColor RgbToHsv(int r, int b, int g)
    {
        double h = 0, s;

        double min = Math.Min(Math.Min(r, g), b);
        double v = Math.Max(Math.Max(r, g), b);
        var delta = v - min;

        if (v == 0.0)
            s = 0;
        else
            s = delta / v;

        if (s == 0)
            h = 0.0;
        else
        {
            if (r == v)
                h = (g - b) / delta;
            else if (g == v)
                h = 2 + (b - r) / delta;
            else if (b == v)
                h = 4 + (r - g) / delta;

            h *= 60;
            if (h < 0.0)
                h = h + 360;
        }

        var hsvColor = new HsvColor { H = h, S = s, V = v / 255 };

        return hsvColor;
    }

    /// <summary>
    /// Converts an HSV color to an RGB color.
    /// </summary>
    /// <param name="hue">Hue</param>
    /// <param name="saturation">Saturation</param>
    /// <param name="value">Value</param>
    /// <param name="alpha">Alpha</param>
    public static Color HsvToRgb(double hue, double saturation, double value, double alpha = 255)
    {
        double red, green, blue;

        if (saturation == 0)
            return Color.FromArgb((byte)alpha, (byte)(value * 255), (byte)(value * 255), (byte)(value * 255));

        if (hue == 360)
            hue = 0;
        else
            hue /= 60;

        var i = (int)Math.Truncate(hue);
        var f = hue - i;

        var p = value * (1.0 - saturation);
        var q = value * (1.0 - saturation * f);
        var t = value * (1.0 - saturation * (1.0 - f));

        switch (i)
        {
            case 0:
                red = value;
                green = t;
                blue = p;
                break;

            case 1:
                red = q;
                green = value;
                blue = p;
                break;

            case 2:
                red = p;
                green = value;
                blue = t;
                break;

            case 3:
                red = p;
                green = q;
                blue = value;
                break;

            case 4:
                red = t;
                green = p;
                blue = value;
                break;

            default:
                red = value;
                green = p;
                blue = q;
                break;
        }

        return Color.FromArgb((byte)alpha, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255));
    }

    /// <summary>
    /// Converts an HSL color to an RGB color.
    /// </summary>
    /// <param name="hue">Hue</param>
    /// <param name="saturation">Saturation</param>
    /// <param name="light">Light</param>
    /// <param name="alpha">Alpha</param>
    public static Color HslToRgb(double hue, double saturation, double light, double alpha = 255)
    {
        if (light == 0)
            return Color.FromArgb((byte)alpha, 0, 0, 0);

        if (saturation == 0)
            return Color.FromArgb((byte)alpha, (byte)light, (byte)light, (byte)light);

        double temp2;

        if (light < 0.5)
            temp2 = light * (1.0 + saturation);
        else
            temp2 = light + saturation - (light * saturation);

        var temp1 = 2.0 * light - temp2;

        double GetColorComponent(double m1, double m2, double m3)
        {
            if (m3 < 0.0)
                m3 += 1.0;
            else if (m3 > 1.0)
                m3 -= 1.0;

            if (m3 < 1.0 / 6.0)
                return m1 + (m2 - m1) * 6.0 * m3;

            if (m3 < 0.5)
                return m2;

            if (m3 < 2.0 / 3.0)
                return m1 + ((m2 - m1) * ((2.0 / 3.0) - m3) * 6.0);

            return m1;
        }

        var r = GetColorComponent(temp1, temp2, hue + 1.0 / 3.0);
        var g = GetColorComponent(temp1, temp2, hue);
        var b = GetColorComponent(temp1, temp2, hue - 1.0 / 3.0);

        return Color.FromArgb((byte)alpha, (byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
    }

    public static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    #endregion

    #region Generation

    /// <summary>
    /// Generates a list of colors with hues ranging from 0-360 and a saturation and value of 1.
    /// </summary>
    /// <returns>The List of Colors</returns>
    public static List<Color> GenerateHsvSpectrum(int count)
    {
        var colorsList = new List<Color>();

        var stop = 360d / count;
        var isDecimal = stop % 1 > 0;

        for (var i = 0; i <= (isDecimal ? count - 1 : count); i++)
            colorsList.Add(HsvToRgb(i * stop, 1, 1));

        if (isDecimal)
            colorsList.Add(HsvToRgb(360, 1, 1));

        //for (var i = 0; i < 29; i++)
        //    colorsList.Add(ConvertHsvToRgb(i * 12, 1, 1, 255));

        //colorsList.Add(ConvertHsvToRgb(0, 1, 1, 255));
        return colorsList;
    }

    /// <summary>
    /// Generates a list of colors with transparency ranging from 0-255.
    /// </summary>
    public static List<Color> GenerateAlphaSpectrum(Color color, int count = 2)
    {
        var colorsList = new List<Color>();

        var stop = 255d / count;
        var isDecimal = stop % 1 > 0;

        for (var i = 0; i <= (isDecimal ? count - 1 : count); i++)
            colorsList.Add(Color.FromArgb((byte)(i * stop), color.R, color.G, color.B));

        if (isDecimal)
            colorsList.Add(Color.FromArgb(255, color.R, color.G, color.B));

        colorsList.Reverse();

        return colorsList;
    }

    public static Color GenerateRandomPastel()
    {
        return HslToRgb(new FastRandom((uint) DateTime.Now.Second).Next(360), 0.75, 0.6);
    }

    #endregion
}