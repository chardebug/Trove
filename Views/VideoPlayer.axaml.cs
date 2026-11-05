using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Trove.VLCPlayback;

namespace Trove.Views;

public partial class VideoPlayer : Window
{
    private readonly VideoPlayerService? _player;

    // Designer / runtime loader only
    public VideoPlayer()
    {
        InitializeComponent();
    }

    public VideoPlayer(string videoPath) : this()
    {
        _player = new VideoPlayerService();

        Opened += (_, _) =>
        {
            try
            {
                VideoView.MediaPlayer = _player.MediaPlayer;
                _player.Play(videoPath);
            }
            //if playback fails, send a message to the debugger
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Trove] Playback failed: {ex}");
                Close();
            }
        };

        //once the window is closed, end the task
        Closed += (_, _) =>
        {
            VideoView.MediaPlayer = null;
            Task.Run(() => _player?.Dispose());
        };
    }
}