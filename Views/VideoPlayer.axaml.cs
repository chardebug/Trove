using System;
using Avalonia.Controls;
using Trove.ViewModels;

namespace Trove.Views;

public partial class VideoPlayer : Window
{
    public VideoPlayer()
    {
        InitializeComponent();
        Opened += (_, _) => (DataContext as VideoPlayerViewModel)?.Play();
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
}
