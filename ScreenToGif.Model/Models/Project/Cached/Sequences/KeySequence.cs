using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences;

/// <summary>
/// Holds key related events (key presses).
/// </summary>
public class KeySequence : RectSequence
{
    public bool KeyStrokesIgnoreNonModifiers { get; set; }

    public bool KeyStrokesIgnoreInjected { get; set; }

    public bool KeyStrokesEarlier { get; set; }

    public double KeyStrokesEarlierBy { get; set; }

    public string KeyStrokesSeparator { get; set; }

    public bool KeyStrokesExtended { get; set; }

    public double KeyStrokesDelay { get; set; }

    public FontFamily KeyStrokesFontFamily { get; set; }

    public FontStyle KeyStrokesFontStyle { get; set; }

    public FontWeight KeyStrokesFontWeight { get; set; }

    public double KeyStrokesFontSize { get; set; }

    public Color KeyStrokesFontColor { get; set; }

    public double KeyStrokesOutlineThickness { get; set; }

    public Color KeyStrokesOutlineColor { get; set; }

    public Color KeyStrokesBackgroundColor { get; set; }

    public VerticalAlignment KeyStrokesVerticalAlignment { get; set; }

    public HorizontalAlignment KeyStrokesHorizontalAlignment { get; set; }

    public double KeyStrokesMargin { get; set; }

    public double KeyStrokesPadding { get; set; }

    public double KeyStrokesMinHeight { get; set; }

    /// <summary>
    /// The list of key events of this sequence.
    /// </summary>
    public List<KeySubSequence> KeyEvents { get; set; } = new();

    public KeySequence()
    {
        Type = SequenceTypes.Key;
    }
}