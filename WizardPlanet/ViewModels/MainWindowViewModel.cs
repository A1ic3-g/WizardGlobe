using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WizardPlanet.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Use the [ObservableProperty] attribute to automatically generate
        // the property and INotifyPropertyChanged logic.
        [ObservableProperty]
        private ViewModelBase? _contentViewModel;
        
        [ObservableProperty]
        private IStorageProvider? _storageProvider;
        
        public MainWindowViewModel()
        {
        }
        public MainWindowViewModel(IStorageProvider storageProvider)
        {
            ContentViewModel = new HomeViewModel(this, storageProvider);
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
        
        [RelayCommand]
        public void NavigateToPlayAnimation()
        {
            ContentViewModel = new PlayAnimationViewModel();
        }
    }
}