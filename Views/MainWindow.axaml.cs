using Avalonia.Controls;

namespace Trove.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Allows the user to click anywhere on the background to exit/deselect the search bar
        PointerPressed += (sender, e) => FocusManager?.Focus(null);
    }
}
