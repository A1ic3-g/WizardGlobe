using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WizardPlanet.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _contentViewModel;

        [ObservableProperty]
        private IStorageProvider? _storageProvider;

        public MainWindowViewModel()
        {
        }

        [RelayCommand]
        public void NavigateToCreateAnimation(IStorageProvider storageProvider)
        {
            ContentViewModel = new CreateAnimationViewModel(storageProvider);
        }

        [RelayCommand]
        private void SetStorageProvider(IStorageProvider storageProvider)
        {
            StorageProvider = storageProvider;
            ContentViewModel = new HomeViewModel(this, StorageProvider);
        }

        // This command shows the selection screen (kept same name for compatibility)
        [RelayCommand]
        public void NavigateToPlayAnimation()
        {
            ContentViewModel = new SelectAnimationViewModel(this);
        }
    }
}