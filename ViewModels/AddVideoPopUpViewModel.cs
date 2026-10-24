using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Trove.ViewModels;

public partial class AddVideoPopUpViewModel : ObservableObject
{
    private readonly Action _onClose;

    // 1. Constructor receives the close action from MainViewModel
        public AddVideoPopUpViewModel(Action onClose)
        {
            _onClose = onClose;
        }   

        // 2. RelayCommand invokes that action when the X button is clicked
        [RelayCommand]
        private void Close()
        {
            _onClose?.Invoke();
        }
}