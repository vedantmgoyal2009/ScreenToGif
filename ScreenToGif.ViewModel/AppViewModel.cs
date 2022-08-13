using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.ViewModels;

namespace ScreenToGif.ViewModel;

public class AppViewModel : BaseViewModel
{
    private DateTime _startupDateTime;
    private bool _ignoreHotKeys;
    private bool _isClearingCache;
    private UpdaterViewModel _updaterViewModel;

    public DateTime StartupDateTime
    {
        get => _startupDateTime;
        set => SetProperty(ref _startupDateTime, value);
    }

    public bool IgnoreHotKeys
    {
        get => _ignoreHotKeys;
        set => SetProperty(ref _ignoreHotKeys, value);
    }

    public bool IsClearingCache
    {
        get => _isClearingCache;
        set => SetProperty(ref _isClearingCache, value);
    }

    public UpdaterViewModel UpdaterViewModel
    {
        get => _updaterViewModel;
        set => SetProperty(ref _updaterViewModel, value);
    }

    //Any other global info.
    //Updates
    //Warnings
    //Notifications

    public IRelayCommand LaunchCommand { get; set; }
    public IRelayCommand StartupCommand { get; set; }
    public IRelayCommand ScreenRecorderCommand { get; set; }
    public IRelayCommand WebcamRecorderCommand { get; set; }
    public IRelayCommand BoardRecorderCommand { get; set; }
    public IRelayCommand TrayLeftClickCommand { get; set; }
    public IRelayCommand TrayLeftDoubleClickCommand { get; set; }
    public IRelayCommand TrayMiddleClickCommand { get; set; }
    public IRelayCommand EditorCommand { get; set; }
    public IRelayCommand UpdateCommand { get; set; }
    public IRelayCommand OptionsCommand { get; set; }
    public IRelayCommand FeedbackCommand { get; set; }
    public IRelayCommand TroubleshootCommand { get; set; }
    public IRelayCommand HelpCommand { get; set; }
    public IRelayCommand ExitCommand { get; set; }

    public IRelayCommand ClearCacheCommand { get; set; }
    public IRelayCommand CheckCacheSpaceCommand { get; set; }
    public IRelayCommand CheckForUpdatesCommand { get; set; }
    public IRelayCommand SendFeedbackCommand { get; set; }

    public AppViewModel()
    {
        StartupDateTime = DateTime.Now;
    }
}