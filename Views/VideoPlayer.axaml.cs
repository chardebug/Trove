using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Trove.ViewModels;

namespace Trove.Views;

public partial class VideoPlayer : Window
{
    private readonly DispatcherTimer _hideTimer = new() { Interval = TimeSpan.FromSeconds(2.5) };
    private VideoPlayerViewModel? _vm;
    private bool _overBar;
    private bool _isDragging;

    // Designer / runtime loader only
    public VideoPlayer()
    {
        InitializeComponent();
    }

    public VideoPlayer(VideoPlayerViewModel vm) : this()
    {
        _vm = vm;
        DataContext = vm;
        OverlayRoot.DataContext = vm;   // the overlay lives in its own window, so set this explicitly

        // Auto-hide
        _hideTimer.Tick += (_, _) => HideControls();
        OverlayRoot.PointerMoved += (_, _) => ShowControls();
        OverlayRoot.PointerPressed += (_, _) => ShowControls();
        ControlBar.PointerEntered += (_, _) => { _overBar = true; ShowControls(); };
        ControlBar.PointerExited += (_, _) => { _overBar = false; ShowControls(); };

        // Show the bar on pause/resume
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(VideoPlayerViewModel.IsPaused)) ShowControls();
        };

        // Seek: forward the drag gesture to the ViewModel
        SeekSlider.AddHandler(PointerPressedEvent, (_, _) =>
        {
            _isDragging = true;
            vm.BeginSeek();
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        SeekSlider.AddHandler(PointerReleasedEvent, (_, _) =>
        {
            _isDragging = false;
            vm.CommitSeek((long)SeekSlider.Value);
            ShowControls();
        }, RoutingStrategies.Bubble, handledEventsToo: true);

        SeekSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty && _isDragging)
                vm.PreviewSeek((long)SeekSlider.Value);
        };

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (_vm is null) return;

        try
        {
            VideoView.MediaPlayer = _vm.MediaPlayer;   // attach first...
            _vm.Start();                               // ...then play
            ShowControls();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Trove] Playback failed: {ex}");
            Close();
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _hideTimer.Stop();

        var vm = _vm;
        _vm = null;

        VideoView.MediaPlayer = null;     // detach first
        DataContext = null;
        OverlayRoot.DataContext = null;

        if (vm is not null)
            Task.Run(vm.Dispose);         // dispose off the UI thread
    }

    private void ShowControls()
    {
        ControlBar.Opacity = 1;
        ControlBar.IsHitTestVisible = true;
        OverlayRoot.Cursor = Cursor.Default;
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    private void HideControls()
    {
        // Stay visible while paused, hovering the bar, or dragging. Each of those
        // calls ShowControls() when it ends, which restarts the timer.
        if (_vm is null || _vm.IsPaused || _overBar || _isDragging)
        {
            _hideTimer.Stop();
            return;
        }

        _hideTimer.Stop();
        ControlBar.Opacity = 0;
        ControlBar.IsHitTestVisible = false;
        OverlayRoot.Cursor = new Cursor(StandardCursorType.None);
    }
}