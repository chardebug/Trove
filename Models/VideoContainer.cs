using CommunityToolkit.Mvvm.ComponentModel;

namespace Trove.Models;

public partial class VideoContainer : ObservableObject
{
    public long ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
