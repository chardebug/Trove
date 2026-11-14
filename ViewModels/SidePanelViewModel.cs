using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Trove.ViewModels;

public partial class SidePanelViewModel(ICommand togglePaneCommand, bool isPaneOpen) : ObservableObject
{
    public ICommand TogglePaneCommand { get; } = togglePaneCommand;

    [ObservableProperty]
    public partial bool IsOpen { get; set; } = isPaneOpen;
    
    [ObservableProperty]
    public partial string SearchText { get; set; } = "";
    
}
