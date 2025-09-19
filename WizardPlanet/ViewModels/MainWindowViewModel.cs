using System.Collections.Generic;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WizardPlanet.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Stack<ViewModelBase> _navigationStack = new();

        private ViewModelBase? _contentViewModel;

        public ViewModelBase? ContentViewModel
        {
            get => _contentViewModel;
            set
            {
                if (ContentViewModel != null)
                {
                    AddToNavigationStack(ContentViewModel);
                }

                SetProperty(ref _contentViewModel, value);
            }
        }

        [ObservableProperty] private IStorageProvider? _storageProvider;

        private void AddToNavigationStack(ViewModelBase? viewModel)
        {
            if (viewModel is null)
            {
                return;
            }

            _navigationStack.Push(viewModel);
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
            ContentViewModel = new SelectAnimationViewModel(this);
        }

        [RelayCommand]
        private void NavigateBack()
        {
            if (_navigationStack.Count == 0) return;
            ContentViewModel = _navigationStack.Pop();
        }
    }
}