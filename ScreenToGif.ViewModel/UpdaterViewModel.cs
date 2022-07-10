using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Settings;

namespace ScreenToGif.ViewModel;

public class UpdaterViewModel : BaseViewModel
{
    private bool _isFromGithub;
    private bool _mustDownloadManually;
    private Version _version;
    private string _description;
    private bool _isDownloading;

    private string _installerPath;
    private string _installerName;
    private string _installerDownloadUrl;
    private long _installerSize;
    private string _portablePath;
    private string _portableName;
    private string _portableDownloadUrl;
    private long _portableSize;

    public bool IsFromGithub
    {
        get => _isFromGithub;
        set => SetProperty(ref _isFromGithub, value);
    }

    public bool MustDownloadManually
    {
        get => _mustDownloadManually;
        set => SetProperty(ref _mustDownloadManually, value);
    }

    public Version Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set => SetProperty(ref _isDownloading, value);
    }

    public string InstallerPath
    {
        get => _installerPath;
        set
        {
            SetProperty(ref _installerPath, value);

            OnPropertyChanged(nameof(ActivePath));
        }
    }

    public string InstallerName
    {
        get => _installerName;
        set
        {
            SetProperty(ref _installerName, value);

            OnPropertyChanged(nameof(ActiveName));
        }
    }

    public string InstallerDownloadUrl
    {
        get => _installerDownloadUrl;
        set
        {
            SetProperty(ref _installerDownloadUrl, value);

            OnPropertyChanged(nameof(ActiveDownloadUrl));
            OnPropertyChanged(nameof(HasDownloadLink));
        }
    }

    public long InstallerSize
    {
        get => _installerSize;
        set
        {
            SetProperty(ref _installerSize, value);

            OnPropertyChanged(nameof(ActiveSize));
        }
    }

    public string PortablePath
    {
        get => _portablePath;
        set
        {
            SetProperty(ref _portablePath, value);

            OnPropertyChanged(nameof(ActivePath));
        }
    }

    public string PortableName
    {
        get => _portableName;
        set
        {
            SetProperty(ref _portableName, value);

            OnPropertyChanged(nameof(ActiveName));
        }
    }

    public string PortableDownloadUrl
    {
        get => _portableDownloadUrl;
        set
        {
            SetProperty(ref _portableDownloadUrl, value);

            OnPropertyChanged(nameof(ActiveDownloadUrl));
            OnPropertyChanged(nameof(HasDownloadLink));
        }
    }

    public long PortableSize
    {
        get => _portableSize;
        set
        {
            SetProperty(ref _portableSize, value);

            OnPropertyChanged(nameof(ActiveSize));
        }
    }

#if FULL_MULTI_MSIX

    public string ActivePath
    {
        get => InstallerPath;
        set => InstallerPath = value;
    }

    public string ActiveName => InstallerName;
    public string ActiveDownloadUrl => InstallerDownloadUrl;
    public long ActiveSize => InstallerSize;

#else

    public string ActivePath
    {
        get => UserSettings.All.PortableUpdate ? PortablePath : InstallerPath;
        set
        {
            if (UserSettings.All.PortableUpdate)
                PortablePath = value;
            else
                InstallerPath = value;
        }
    }

    public string ActiveName => UserSettings.All.PortableUpdate ? PortableName : InstallerName;
    public string ActiveDownloadUrl => UserSettings.All.PortableUpdate ? PortableDownloadUrl : InstallerDownloadUrl;
    public long ActiveSize => UserSettings.All.PortableUpdate ? PortableSize : InstallerSize;

#endif

    public bool HasDownloadLink => !string.IsNullOrWhiteSpace(ActiveDownloadUrl);
}