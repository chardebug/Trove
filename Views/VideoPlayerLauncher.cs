//This creates the window

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Trove.Services;
using Trove.ViewModels;

namespace Trove.Views;

public sealed class VideoPlayerLauncher : IVideoPlayerLauncher
{
    public void Play(string videoPath)
    {
        var owner = (Application.Current?.ApplicationLifetime
                        as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var window = new VideoPlayer(new VideoPlayerViewModel(videoPath));

        if (owner is not null) window.Show(owner);
        else window.Show();
    }
}