using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using WizardPlanet.Models;

namespace WizardPlanet.ViewModels;
public partial class SelectAnimationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<AnimationGridRowViewModel> _animationViewModels;

    private readonly MainWindowViewModel _mainWindow;
    private List<GlobeAnimation> _animations;

    public SelectAnimationViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;

        var animsPath = Path.Combine("Animations", "animations.json");
        _animations = JsonSerializer.Deserialize<List<GlobeAnimation>>(File.ReadAllText(animsPath)) ?? [];
        AnimationViewModels = [];
        foreach (var anim in _animations)
        {
            AnimationViewModels.Add(new AnimationGridRowViewModel(anim, _mainWindow));
        }
    }
}