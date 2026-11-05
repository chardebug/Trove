using System;
using LibVLCSharp.Shared;

namespace Trove.ViewModels;

public class VideoPlayerViewModel : IDisposable
{
    private readonly LibVLC _libVLC = new();
    private readonly string _videoPath;

    public MediaPlayer MediaPlayer { get; }

    public VideoPlayerViewModel(string videoPath)
    {
        _videoPath = videoPath;
        MediaPlayer = new MediaPlayer(_libVLC);
    }

    public void Play()
    {
        using var media = new Media(_libVLC, _videoPath, FromType.FromPath);
        MediaPlayer.Play(media);
    }

    public void Dispose()
    {
        MediaPlayer.Stop();
        MediaPlayer.Dispose();
        _libVLC.Dispose();
    }
}