using System;
using System.IO;
using LibVLCSharp.Shared;

namespace Trove.VLCPlayback;

public sealed class VideoPlayerService : IDisposable
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;
    private Media? _currentMedia;
    private bool _disposed;

    //start a new instance of the player
    public VideoPlayerService()
    {
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
    }

    public MediaPlayer MediaPlayer => _mediaPlayer;

    //play the video
    public void Play(string mediaPath)
    {

        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaPath);

        //error checking if file doesn't exist
        if (!File.Exists(mediaPath))
        {
            throw new FileNotFoundException("The video file could not be found.", mediaPath);
        }

        var media = new Media(_libVlc, mediaPath, FromType.FromPath);
        _mediaPlayer.Stop();
        _currentMedia?.Dispose();
        _currentMedia = null;

        //throw an exception if the video couldn't be played
        try
        {
            if (!_mediaPlayer.Play(media))
            {
                throw new InvalidOperationException($"VLC could not start playback for '{mediaPath}'.");
            }

            _currentMedia = media;
        }
        catch
        {
            media.Dispose();
            throw;
        }
    }

    //pause the video
    public void Pause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _mediaPlayer.Pause();
    }

    //stop the video
    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _mediaPlayer.Stop();
    }


    //once the window is closed, clean up
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _mediaPlayer.Stop();
        _currentMedia?.Dispose();
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
        _disposed = true;
    }
}