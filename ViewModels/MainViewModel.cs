using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Trove.ViewModels;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty]
    public partial bool IsPaneOpen { get; set; } = true;

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    public SidePanelViewModel SidePanel { get; }

    public MainViewModel()
    {
        SidePanel = new SidePanelViewModel(TogglePaneCommand, IsPaneOpen);
    }

    partial void OnIsPaneOpenChanged(bool value)
    {
        SidePanel.IsOpen = value;
    }

    

}
