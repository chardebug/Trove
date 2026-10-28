using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Trove.Models;
using System.Collections.ObjectModel;

namespace Trove.ViewModels;

public partial class MainViewModel : ObservableObject
{
    //Side Panel toggling
    [ObservableProperty]
    public partial bool IsPaneOpen { get; set; } = true;

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    public SidePanelViewModel SidePanel { get; }

    partial void OnIsPaneOpenChanged(bool value)
    {
        if(SidePanel != null)
        {
            SidePanel.IsOpen = value;
        }
    }

    //Add button file picker
    private readonly IStorageProvider _storageProvider;
    [ObservableProperty]
    private string? _selectedVideoPath;

    public MainViewModel() : this(null!)
    {
        
    }

    public MainViewModel(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
        SidePanel = new SidePanelViewModel(TogglePaneCommand, IsPaneOpen);
    }

    //Video container collection
    public ObservableCollection<VideoContainer> Videos { get; } = [];

    [ObservableProperty]
    private string? path = "None";

    [ObservableProperty]
    private string _videoTitle = string.Empty;
    [RelayCommand]

    //Opens OS file picker window and extracts file path
    
    private async Task OpenFileAsync()
    {
        //only allow video files
        var videoFileType = new FilePickerFileType("Video Files")
        {
            Patterns = ["*.mp4", "*.mkv", "*.avi", "*.flv"],
            MimeTypes = ["video/* "]
        };

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Video File",
            AllowMultiple = false,
            FileTypeFilter = [videoFileType]
        });
        if (files.Count > 0)
        {
            //here is the file path
            Path = files[0].TryGetLocalPath();
        }
    }

    string defaultThumbnail = "avares://Trove/Assets/VideoPlaceholderImage.png";

    //add video button control
    [RelayCommand]
    private void AddVideo()
    {
        //create a new VideoContainer
        var newVideo = new VideoContainer
        {
            Title = VideoTitle,
            VideoPath = Path ?? string.Empty,
            ThumbnailPath = defaultThumbnail
        };

        //update grid
        Videos.Add(newVideo);

        //clear inputs and close popup
        VideoTitle = string.Empty;
        Path = string.Empty;
        IsPopUpVisible = false;

    }

    //pop-up window toggles
    [ObservableProperty]
    private bool _IsPopUpVisible;

    [RelayCommand]
    private void OpenPopUp()
    {
        IsPopUpVisible = true;
    }

    [RelayCommand]
    private void ClosePopUp()
    {
        IsPopUpVisible = false;
        Path = string.Empty;
    }


}

