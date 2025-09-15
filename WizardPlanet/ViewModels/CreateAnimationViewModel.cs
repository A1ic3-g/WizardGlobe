// Inside your CreateAnimationViewModel.cs

using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using WizardPlanet.Services;

namespace WizardPlanet.ViewModels;
public partial class CreateAnimationViewModel(IStorageProvider storageProvider) : ViewModelBase
{
    private readonly IStorageProvider _storageProvider = storageProvider;
    private IGlobeGenerationService? _globeGenerationService;
    [ObservableProperty]
    private string? _inputMapPath;

    [ObservableProperty]
    private string? _animationName;

    [ObservableProperty]
    private string? _animationDescription;

    [RelayCommand]
    private async Task Browse()
    {
        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Image File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Image files")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                }
            }
        });

        if (files.Count >= 1)
        {
            // Get the path of the selected file
            InputMapPath = files[0].Path.AbsolutePath;
        }
    }

    [RelayCommand]
    private async Task CreateAnimation()
    {
        if (File.Exists(InputMapPath) && !string.IsNullOrWhiteSpace(AnimationName))
        {
            var animDir = Directory.CreateDirectory(Path.Combine("Animations", AnimationName));
            
            _globeGenerationService ??= new GlobeGenerationService();

            await _globeGenerationService.CreateGlobeRotationSequenceAsync(InputMapPath, animDir.FullName, 360);
        }
    }
}