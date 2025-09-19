// Inside your CreateAnimationViewModel.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using WizardPlanet.Models;
using WizardPlanet.Services;

namespace WizardPlanet.ViewModels;
public partial class CreateAnimationViewModel(IStorageProvider storageProvider) : ViewModelBase
{
    private const int NoFrames = 360;
    private readonly IStorageProvider _storageProvider = storageProvider;
    private IGlobeGenerationService? _globeGenerationService;
    [ObservableProperty]
    private string? _inputMapPath;

    [ObservableProperty]
    private string? _animationName;

    [ObservableProperty]
    private string? _animationDescription;
    
    [ObservableProperty]
    private string? _statusMessage;

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
        if (!File.Exists(InputMapPath) || string.IsNullOrWhiteSpace(AnimationName))
        {
            StatusMessage = "Please select an input image and enter an animation name";
            return;
        }

        var animDir = Directory.CreateDirectory(Path.Combine("Animations", AnimationName));
            
        _globeGenerationService ??= new GlobeGenerationService();
        StatusMessage = "Creating animation...";
        await _globeGenerationService.CreateGlobeRotationSequenceAsync(InputMapPath, animDir.FullName, NoFrames);

        var anim = new GlobeAnimation()
        {
            Name = AnimationName,
            Description = AnimationDescription,
            DirectoryPath = animDir.Name,
            NoFrames = NoFrames
        };
        
        var animsPath = Path.Combine("Animations", "animations.json");
        var anims = new List<GlobeAnimation>();
        if (File.Exists(animsPath))
        {
            anims = JsonSerializer.Deserialize<List<GlobeAnimation>>(await File.ReadAllTextAsync(animsPath));
            anims = anims?.Where(a => a.Name != anim.Name).ToList() ?? [];
        }

        anims.Add(anim);
        var animsJson = JsonSerializer.Serialize(anims);
        await File.WriteAllTextAsync(animsPath, animsJson);
        StatusMessage = "Animation created!";
    }
}