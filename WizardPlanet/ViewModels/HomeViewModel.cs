using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace WizardPlanet.ViewModels
{
    public partial class HomeViewModel(MainWindowViewModel mainWindowViewModel, IStorageProvider storageProvider)
        : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel = mainWindowViewModel;
        private readonly IStorageProvider _storageProvider = storageProvider;

        [RelayCommand]
        private void CreateAnimation()
        {
            _mainWindowViewModel.NavigateToCreateAnimationCommand.Execute(_storageProvider);
        }
        
        [RelayCommand]
        private void PlayAnimation()
        {
            _mainWindowViewModel.NavigateToPlayAnimationCommand.Execute(null);
        }
    }
}