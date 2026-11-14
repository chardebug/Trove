using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Trove.VLCPlayback;

namespace Trove.ViewModels;

public partial class VideoPlayerViewModel : ObservableObject, IDisposable
{
    private readonly VideoPlayerService _player = new();
    private readonly string _videoPath;
    private readonly SynchronizationContext? _ui = SynchronizationContext.Current;

    private volatile bool _disposed;
    private bool _ended;
    private bool _isSeeking;

    public MediaPlayer MediaPlayer => _player.MediaPlayer;

    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private string _playPauseLabel = "⏸";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PositionText))]
    private long _position;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(LengthText))]
    private long _length = 1;

    public string PositionText => Format(Position);
    public string LengthText => Format(Length);

    public VideoPlayerViewModel(string videoPath)
    {
        _videoPath = videoPath;
        var mp = _player.MediaPlayer;

        // VLC raises these on its own threads: marshal to the UI thread, and never
        // call back into the MediaPlayer from inside them.
        mp.LengthChanged += (_, e) => PostUi(() => Length = Math.Max(1, e.Length));

        mp.TimeChanged += (_, e) => PostUi(() =>
        {
            if (!_isSeeking) Position = e.Time;   // don't fight the user's drag
        });

        mp.Playing += (_, _) => PostUi(() =>
        {
            IsPaused = false;
            PlayPauseLabel = "⏸";
        });

        mp.Paused += (_, _) => PostUi(() =>
        {
            IsPaused = true;
            PlayPauseLabel = "▶";
        });

        mp.EndReached += (_, _) => PostUi(() =>
        {
            _ended = true;
            IsPaused = true;
            PlayPauseLabel = "🔄";
        });
    }

    public void Start() => _player.Play(_videoPath);

    [RelayCommand]
    private void TogglePlayPause()
    {
        if (_ended)
        {
            _ended = false;
            _player.Play(_videoPath);
        }
        else
        {
            _player.Pause();   // toggles pause/resume
        }
    }

    // Seeking: the view forwards the user's drag gesture to these three methods.
    public void BeginSeek() => _isSeeking = true;
    public void PreviewSeek(long ms) => Position = ms;

    public void CommitSeek(long ms)
    {
        Position = ms;
        _isSeeking = false;
        _player.MediaPlayer.Time = ms;
    }

    public void Dispose()
    {
        _disposed = true;
        _player.Dispose();
    }

    private void PostUi(Action action)
    {
        void Run() { if (!_disposed) action(); }

        if (_ui is null) Run();
        else _ui.Post(_ => Run(), null);
    }

    private static string Format(long ms)
    {
        var t = TimeSpan.FromMilliseconds(ms);
        return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");
    }
}