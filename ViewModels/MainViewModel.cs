using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Trove.Models;
using System.Collections.ObjectModel;
using System.IO;
using Trove.Services;
using System.Linq;


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

    private readonly IVideoPlayerLauncher _videoPlayer;
    [ObservableProperty]
    private string? _selectedVideoPath;

    public MainViewModel() : this(null!, null!)
    {
        
    }

    public MainViewModel(IStorageProvider storageProvider, IVideoPlayerLauncher videoPlayer)
    {
        _storageProvider = storageProvider;
        _videoPlayer = videoPlayer;
        SidePanel = new SidePanelViewModel(TogglePaneCommand, IsPaneOpen);
    }

    //Video container collection
    public ObservableCollection<VideoContainer> Videos { get; } = [];

    [ObservableProperty]
    private string? videoFilePath = "None";

    [ObservableProperty]
    private string? thumbnailFilePath = "None";

    [ObservableProperty]
    private string videoFileName = "None";

    [ObservableProperty]
    private string thumbnailFileName = "None";

    [ObservableProperty]
    private string _videoTitle = string.Empty;
    [RelayCommand]

    //Opens OS file picker window and extracts file path
    
    private async Task OpenVideoFileAsync()
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
            VideoFilePath = files[0].TryGetLocalPath();
            VideoFileName = files[0].Name;
        }
    }

    [RelayCommand]
    private async Task OpenThumbnailFileAsync()
    {

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Thumbnail File",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });
        if (files.Count > 0)
        { 
            //here is the file path
            ThumbnailFilePath = files[0].TryGetLocalPath();
            ThumbnailFileName = files[0].Name;
        }
    }

    //add video button control
    [RelayCommand]
    private void AddVideo()
    
    {
        //create a new VideoContainer
        var newVideo = new VideoContainer
        {
            Title = VideoTitle,
            VideoPath = VideoFilePath ?? string.Empty,
            ThumbnailPath = ThumbnailFilePath ?? string.Empty
        };

        //update grid
        Videos.Add(newVideo);

        //clear inputs and close popup
        VideoTitle = string.Empty;
        VideoFilePath = string.Empty;
        ThumbnailFilePath = string.Empty;
        VideoFileName = "None";
        ThumbnailFileName = "None";
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
        ThumbnailFilePath = string.Empty;
        VideoFilePath = string.Empty;
        VideoFileName = "None";
        ThumbnailFileName = "None";
    }

    //confirmation window toggles

    [ObservableProperty]
    private bool _IsConfirmationVisible;

    [RelayCommand]
    private void OpenConfirmation()
    {
        IsConfirmationVisible = true;
    }

    [RelayCommand]
    private void CloseConfirmation()
    {
        IsConfirmationVisible = false;
    }

    //Select controls
    [ObservableProperty]
    private bool _IsSelectButtonVisible = true;

    [ObservableProperty]
    private bool _IsDeleteButtonVisible = false;

    [ObservableProperty]
    private bool _IsCancelButtonVisible = false;

    //if selection mode is on, do not play videos
    [ObservableProperty]  
    [NotifyPropertyChangedFor(nameof(CanPlay), nameof(SelectButtonText))]
    [NotifyCanExecuteChangedFor(nameof(PlayVideoCommand))]
    private bool _isSelectionMode;

    public bool CanPlay => !IsSelectionMode;
    public string SelectButtonText => IsSelectionMode ? "Cancel" : "Select";

    [RelayCommand]
    private void ToggleSelectionMode()
    {
        IsSelectButtonVisible = !IsSelectButtonVisible;
        IsSelectionMode = !IsSelectionMode;
        IsDeleteButtonVisible = !IsDeleteButtonVisible;
        IsCancelButtonVisible = !IsCancelButtonVisible;

        //to do: make checkboxes visible
    }

    partial void OnIsSelectionModeChanged(bool value)
    {
        if (value) return;
        foreach (var v in Videos) v.IsSelected = false;
    }

    //Video player command
    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void PlayVideo(VideoContainer? video)
    {
        if (video is null || string.IsNullOrWhiteSpace(video.VideoPath)) return;
        _videoPlayer.Play(video.VideoPath);
    }

    

    [RelayCommand]
    private void ConfirmDelete()
    {
        var toRemove = Videos.Where(v => v.IsSelected).ToList();
        foreach (var v in toRemove) Videos.Remove(v);
        ToggleSelectionMode();
        IsConfirmationVisible = false;
}

    [RelayCommand]
    private void Cancel()
    {
        ToggleSelectionMode();
    }

    
}



