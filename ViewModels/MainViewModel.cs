using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Trove.Models;
using System.Collections.ObjectModel;
using System.IO;
using System;
using Trove.Services;
using System.Linq;
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
    private readonly IVideoPlayerLauncher _videoPlayer;
    [ObservableProperty]
    private string? _selectedVideoPath;

    public MainViewModel() : this(null, null!)
    {
        
    }

    public MainViewModel(IStorageProvider? storageProvider, IVideoPlayerLauncher videoPlayer)
    {
        _storageProvider = storageProvider;
        _videoPlayer = videoPlayer;
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
                allVideos.Add(video);
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

    // Stores whatever text the user types into the search box
    [ObservableProperty]
    public string searchText="";
    
    // Toolkit trigger: calls Search() whenever the search text changes
    partial void OnSearchTextChanged(string value)
    {
        Search();
    }
    
    // Master list of all videos in the library (prevents losing videos when filtering)
    private List<VideoContainer>allVideos = new();

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
            ThumbnailFilePath = files[0].TryGetLocalPath();
            ThumbnailFileName = files[0].Name;
        }
    }

    //add video button control
    [RelayCommand]
    private void AddVideo()
    
    {
        //create a new VideoContainer
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
            ThumbnailFilePath = "avares://Trove/Assets/VideoPlaceholderImage.png";
            //return;
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
            allVideos.Add(newVideo);
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
        foreach (var v in toRemove) 
        {
            Videos.Remove(v);
            allVideos.Remove(v); // Removes video from master backup list as well
        }
        ToggleSelectionMode();
        IsConfirmationVisible = false;
    }

    [RelayCommand]
    private void Cancel()
    {
        ToggleSelectionMode();
    }

    // Search implementation: filters videos in real-time
    [RelayCommand]
    private void Search()
    {
        // Clear the current screen display
         Videos.Clear();

        // If the user cleared the box, restore the full library
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var video in allVideos)
            {
                Videos.Add(video);
            }
            return; 
        }

        // Find and add matching videos (case-insensitive)
        foreach(var video in allVideos)
        {
            if (video.Title.ToLower().StartsWith(SearchText.ToLower()))
            {
                Videos.Add(video);
            }
        }
    }

    // Resets search input and restores the original video library
    [RelayCommand]
    private void ResetSearch()
    {
        SearchText = "";

    }
    
}



