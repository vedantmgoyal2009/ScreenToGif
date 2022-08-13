using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Controls;

public class StatusList : StackPanel
{
    #region Dependency Properties/Events

    public static readonly DependencyProperty MaxBandsProperty = DependencyProperty.Register("MaxBands", typeof(int), typeof(StatusBand),
        new FrameworkPropertyMetadata(5));

    #endregion

    #region Properties

    [Bindable(true), Category("Common")]
    public int MaxBands
    {
        get => (int)GetValue(MaxBandsProperty);
        set => SetValue(MaxBandsProperty, value);
    }

    #endregion

    private void Add(StatusTypes type, string text, StatusReasons reason, Action action = null)
    {
        var current = Children.OfType<StatusBand>().FirstOrDefault(x => x.Type == type && x.Text == text);

        if (current != null)
            Children.Remove(current);

        var band = new StatusBand { Reason = reason };
        band.Dismissed += (_, _) => Children.Remove(band);

        if (Children.Count >= MaxBands)
            Children.RemoveAt(0);

        Children.Add(band);

        switch (type)
        {
            case StatusTypes.Info:
                band.Info(text, action);
                break;
            case StatusTypes.Warning:
                band.Warning(text, action);
                break;
            case StatusTypes.Error:
                band.Error(text, action);
                break;
        }
    }

    public void Info(string text, StatusReasons reason = StatusReasons.None, Action action = null)
    {
        Add(StatusTypes.Info, text, reason, action);
    }

    public void Warning(string text, StatusReasons reason = StatusReasons.InvalidState, Action action = null)
    {
        Add(StatusTypes.Warning, text, reason, action);
    }

    public void Error(string text, StatusReasons reason, Action action = null)
    {
        Add(StatusTypes.Error, text, reason, action);
    }

    public void Remove(StatusTypes type, StatusReasons? reason = null)
    {
        var list = Children.OfType<StatusBand>().Where(x => x.Type == type && (!reason.HasValue || x.Reason == reason)).ToList();

        foreach (var band in list)
            Children.Remove(band);
    }

    public void Clear()
    {
        Children.Clear();
    }
}