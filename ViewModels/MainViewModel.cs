using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.Reflection;
using Trove.Views;

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
    //TODO make this button open the pop-up menu instead
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

    [ObservableProperty]
    private string? path = "None";
    [RelayCommand]
    //Opens OS file picker window and extracts file path
    // TODO: move this into the pop-up window functionality, button will instead open the pop-up
    
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
            string? path = files[0].TryGetLocalPath();
        }
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
        Path = "None";
    }
}

