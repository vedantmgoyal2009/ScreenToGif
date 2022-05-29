using ScreenToGif.Domain.Models.Project.Recording;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Project;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using ScreenToGif.ViewModel.Project;
using System.Diagnostics;

namespace ScreenToGif.ViewModel;

public class EditorViewModel : BaseViewModel
{
    #region Variables

    private ProjectViewModel _project;
    private TimeSpan _current = TimeSpan.Zero;
    private int _currentIndex = -1;
    private WriteableBitmap _renderedImage;
    private IntPtr _renderedImageBackBuffer;
    private double _zoom = 1d;
    private double _quality = 1d;
    private double _viewportTop = 0d;
    private double _viewportLeft = 0d;
    private double _viewportWidth = 0d;
    private double _viewportHeigth = 0d;
    private bool _isLoading;

    //Erase it later.
    private ObservableCollection<FrameViewModel> _frames = new();

    #endregion

    #region Properties

    public CommandBindingCollection CommandBindings => new()
    {
        new CommandBinding(FindCommand("Command.NewRecording"), (sender, args) => { Console.WriteLine(""); }, (sender, args) => { args.CanExecute = true; }),
        new CommandBinding(FindCommand("Command.NewWebcamRecording"), (sender, args) => { Console.WriteLine(""); }, (sender, args) => { args.CanExecute = true; }),
    };

    public ProjectViewModel Project
    {
        get => _project;
        set => SetProperty(ref _project, value);
    }

    public TimeSpan Current
    {
        get => _current;
        set
        {
            SetProperty(ref _current, value);
            Seek();
        }
    }

    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }

    public WriteableBitmap RenderedImage
    {
        get => _renderedImage;
        set
        {
            SetProperty(ref _renderedImage, value);

            _renderedImageBackBuffer = value.BackBuffer;
        }
    }

    public double Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, value);
    }

    public double Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public double ViewportTop
    {
        get => _viewportTop;
        set => SetProperty(ref _viewportTop, value);
    }

    public double ViewportLeft
    {
        get => _viewportLeft;
        set => SetProperty(ref _viewportLeft, value);
    }

    public double ViewportWidth
    {
        get => _viewportWidth;
        set => SetProperty(ref _viewportWidth, value);
    }

    public double ViewportHeigth
    {
        get => _viewportHeigth;
        set => SetProperty(ref _viewportHeigth, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// The list of frames. TODO: Erase it later.
    /// </summary>
    public ObservableCollection<FrameViewModel> Frames
    {
        get => _frames;
        set => SetProperty(ref _frames, value);
    }

    #endregion

    public EditorViewModel()
    {
        //?
    }

    #region Methods

    public async Task ImportFromRecording(RecordingProject project)
    {
        IsLoading = true;

        //Show progress.
        //  Create list of progresses.
        //  Pass the created progress reporter.
        //Cancelable.
        //  Pass token.

        var cached = await project.ConvertToCachedProject();
        Project = ProjectViewModel.FromModel(cached);

        InitializePreview();
        
        IsLoading = false;
    }

    internal void InitializePreview()
    {
        RenderedImage = new WriteableBitmap(Project.Width, Project.Height, Project.HorizontalDpi, Project.VerticalDpi, PixelFormats.Bgra32, null);

        Render();
    }

    internal void Render()
    {
        if (RenderedImage == null)
            return;

        //Pre-render adjustments:
        //  Adjust rendering based on zoom, position, and size.
        //  Quality (maybe, as a plus later).

        //How to render?
        //  Directly to WriteableBitmap address.
        //  Only render what's inside the canvas.
        //  Some sequences can be resized and have a defined rendering size.
        //  Maybe: Viewport details need to be passed along so that the rendering is accurate and within bounds.

        //After rendering?
        //  Cache somehow?
        //      I only need to cache the result frames or the layers.
        //          But since this app will work based on timestamp, how to decide what to render?
        //          Based on changes? Frame event or other event.
        //          Sequences are going to have internal FPS.
        //          Sequence rendering will probably have a high cost, specially because there's tons of data.? 
        //      
        //      MemoryCache
        //      or
        //      CachedContent<T>
        //          Id
        //          IsValid
        //  Invalidate cache
        //      Mark cache list as invalid and request render again.

        //using (var context = RenderedImage.GetBitmapContext())
        //    RenderedImage.DrawRectangle(0, 0, 100, 100, 100);

        System.Diagnostics.Debug.WriteLine("Render start");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        DrawBackground();

        foreach (var track in Project.Tracks)
            track.RenderAt(_renderedImageBackBuffer, Project.Width, Project.Height, Current, Quality);
        
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            RenderedImage.Lock();
            RenderedImage.AddDirtyRect(new Int32Rect(0, 0, RenderedImage.PixelWidth, RenderedImage.PixelHeight));
            RenderedImage.Unlock();
        }, DispatcherPriority.Render);

        System.Diagnostics.Debug.WriteLine("Finished: " + sw.Elapsed);

        //How are previews going to work?
        //  Text rendering
        //  Rendering that needs access to the all layers.
        //  Rendering that changes the size of the canvas.

        //Preview quality.
        //Render the list preview for the frames.

        //Decorator Layer
        //  Needs access to the position, size and angle of the sequence objects.
        //  Altering the size/position/angle needs to directly alter the value of the sequences and subsequences.
        //  Maybe pass Project directly to the decorator layer and let that control read/change the values.
    }

    private void DrawBackground()
    {
        //Project.Background = new LinearGradientBrush(Colors.Yellow, Colors.Black, new Point(0,0.5), new Point(1,0.5));

        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
            drawingContext.DrawRectangle(Project.Background, null, new Rect(0, 0, Project.Width, Project.Height));
        
        var target = new RenderTargetBitmap(Project.Width, Project.Height, Project.HorizontalDpi, Project.VerticalDpi, PixelFormats.Pbgra32);
        target.Render(drawingVisual);

        //Size * channels * Bytes per pixel + (height * stride padding?);
        //var buffer = new byte[RenderedImage.BackBufferStride * RenderedImage.PixelHeight];
        //target.CopyPixels(buffer, RenderedImage.BackBufferStride, 0);

        target.CopyPixels(new Int32Rect(0, 0, Project.Width, Project.Height), _renderedImageBackBuffer, RenderedImage.BackBufferStride * RenderedImage.PixelHeight, RenderedImage.BackBufferStride);
        
        //How to cache this?
        //Maybe simply store in a byte array and leave in memory.
    }

    public void Seek()
    {
        //Display mode:
        //  By timestamp
        //      Preview is controlled by a timestamp.
        //  By frame selection
        //      Preview is controlled by a timestamp, but users are actually selecting frames (just that each frame has its own frame timestamp).
        //      So the only thing that changes is how the user sees and seeks the recording.

        //By seeking, display the updated info in Statistic tab.
        //Frame count will be hard to know for sure, as multiple sequences can coexist and apart from each other.

        Render();
    }

    internal void Play()
    {
        //Clock based on a selected fps.
        //Maybe variable? By detecting the sub-sequences.
    }

    //How are the frames/data going to be stored in the disk?
    //Project file for the user + opened project should have a cache
    //  Project file for user: I'll need to create a file spec.
    //  Cache folder for the app:

    //As a single cache for each track? (storing as pixel array, to improve performance)
    //I'll need a companion json with positions and other details.
    //I also need to store in memory for faster usage.

    #endregion
}