using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Trove.Models;
using Trove.Views;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes; 
using Microsoft.Data.Sqlite;
using Trove.Database;

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
    private readonly IStorageProvider? _storageProvider;
    private readonly VideoRepository? _videoRepository;

    [ObservableProperty]
    private string? _selectedVideoPath;

    public MainViewModel() : this(null)
    {
        
    }

    public MainViewModel(IStorageProvider? storageProvider)
    {
        _storageProvider = storageProvider;
        SidePanel = new SidePanelViewModel(TogglePaneCommand, IsPaneOpen);

        if (_storageProvider is null)
        {
            return;
        }

        try
        {
            _videoRepository = new VideoRepository();
            foreach (var video in _videoRepository.GetAllVideos())
            {
                Videos.Add(video);
            }
        }
        catch (SqliteException ex)
        {
            StatusMessage = $"Could not load the video library: {ex.Message}";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Could not access the video database: {ex.Message}";
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = $"Access to the video database was denied: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Could not initialize the video database: {ex.Message}";
        }
    }

    //Video container collection
    public ObservableCollection<VideoContainer> Videos { get; } = [];

    [ObservableProperty]
    private string? videoFilePath = string.Empty;

    [ObservableProperty]
    private string? thumbnailFilePath = string.Empty;

    [ObservableProperty]
    private string videoFileName = "None";

    [ObservableProperty]
    private string thumbnailFileName = "None";

    [ObservableProperty]
    private string _videoTitle = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]

    //Opens OS file picker window and extracts file path
    
    private async Task OpenVideoFileAsync()
    {
        if (_storageProvider is null)
        {
            return;
        }

        //only allow video files
        var videoFileType = new FilePickerFileType("Video Files")
        {
            Patterns = ["*.mp4", "*.mkv", "*.avi", "*.flv"],
            MimeTypes = ["video/*"]
        };

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Video File",
            AllowMultiple = false,
            FileTypeFilter = [videoFileType]
        });
        if (files.Count > 0)
        {
            var path = files[0].TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                StatusMessage = "The selected video does not have a local file path.";
                return;
            }

            VideoFilePath = path;
            VideoFileName = files[0].Name;
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task OpenThumbnailFileAsync()
    {
        if (_storageProvider is null)
        {
            return;
        }

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Thumbnail File",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });
        if (files.Count > 0)
        {
            var path = files[0].TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                StatusMessage = "The selected thumbnail does not have a local file path.";
                return;
            }

            ThumbnailFilePath = path;
            ThumbnailFileName = files[0].Name;
            StatusMessage = string.Empty;
        }
    }

    //add video button control
    [RelayCommand]
    private void AddVideo()
    {
        if (_videoRepository is null)
        {
            StatusMessage = "The video library database is unavailable. Restart the app after resolving the database error.";
            return;
        }

        var title = VideoTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            StatusMessage = "Enter a title for the video.";
            return;
        }

        if (string.IsNullOrWhiteSpace(VideoFilePath) || !File.Exists(VideoFilePath))
        {
            StatusMessage = "Select an existing video file before adding it.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(ThumbnailFilePath) && !File.Exists(ThumbnailFilePath))
        {
            StatusMessage = "The selected thumbnail file could not be found.";
            return;
        }

        var newVideo = new VideoContainer
        {
            Title = title,
            VideoPath = VideoFilePath,
            ThumbnailPath = ThumbnailFilePath ?? string.Empty
        };

        try
        {
            newVideo.ID = _videoRepository.AddVideo(newVideo);
            Videos.Add(newVideo);
        }
        catch (SqliteException ex)
        {
            StatusMessage = $"Could not save the video: {ex.Message}";
            return;
        }
        catch (IOException ex)
        {
            StatusMessage = $"Could not access the video database: {ex.Message}";
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = $"Access to the video database was denied: {ex.Message}";
            return;
        }

        //clear inputs and close popup
        VideoTitle = string.Empty;
        VideoFilePath = string.Empty;
        ThumbnailFilePath = string.Empty;
        VideoFileName = "None";
        ThumbnailFileName = "None";
        IsPopUpVisible = false;
        StatusMessage = string.Empty;

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
        StatusMessage = string.Empty;
    }

    //Video player command
    [RelayCommand]
    private void PlayVideo(VideoContainer? video)
    {
        //exit if video path is null or empty
        if (video is null || string.IsNullOrWhiteSpace(video.VideoPath)) return;

        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var player = new VideoPlayer(video.VideoPath);

        if (owner is not null) player.Show(owner);
        else player.Show();
    }

}
