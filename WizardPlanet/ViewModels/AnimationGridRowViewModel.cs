using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WizardPlanet.Models;

namespace WizardPlanet.ViewModels;

public partial class AnimationGridRowViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    [ObservableProperty]
    private GlobeAnimation _animation;

    [ObservableProperty]
    private string? _previewImagePath;

    public AnimationGridRowViewModel(GlobeAnimation animation, MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
        Animation = animation;
        if (animation.DirectoryPath == null) return;
        PreviewImagePath = Path.Combine("Animations", animation.DirectoryPath, "original.png");
    }

    [RelayCommand]
    private void NavigateToPlayAnimation()
    {
        // If your PlayAnimationViewModel needs the selected animation, pass it here.
        // e.g., new PlayAnimationViewModel(Animation)
        _mainWindow.ContentViewModel = new PlayAnimationViewModel(Animation);
    }
}