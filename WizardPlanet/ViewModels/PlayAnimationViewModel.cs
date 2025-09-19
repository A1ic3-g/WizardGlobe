// ViewModels/PlayAnimationViewModel.cs
using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media.Imaging;
using WizardPlanet.Models;
using WizardPlanet.Views;
using Avalonia.Threading;
using Avalonia.Controls;

namespace WizardPlanet.ViewModels;

public partial class PlayAnimationViewModel : ViewModelBase
{
    public GlobeAnimation Animation { get; }
    private readonly string _framesDir;

    [ObservableProperty] private int _frameIndex;
    [ObservableProperty] private Bitmap? _currentFrameBitmap;

    private PlaybackWindow? _window;
    private DispatcherTimer? _timer;
    private int _totalFrames;
    private double _speedMultiplier = 1.0; // 1.0 = 10s per loop baseline

    public int TotalFrames => _totalFrames;
    public double SpeedMultiplier => _speedMultiplier;

    public PlayAnimationViewModel(GlobeAnimation animation)
    {
        Animation = animation;
        _framesDir = Path.Combine("Animations", animation.DirectoryPath ?? string.Empty);

        // Count frames (exclude original.png which doesn't match the pattern)
        _totalFrames = CountFrames();

        // Start at the first frame and open the playback window.
        FrameIndex = 0;
        LoadFrameOrWrap();

        // Initialize timer and start autoplay looping
        InitTimer();
        StartTimer();

        OpenWindow();
    }

    private void InitTimer()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => AdvanceFrame();
        UpdateTimerInterval();
    }

    private void StartTimer()
    {
        if (_timer is { IsEnabled: false })
            _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is { IsEnabled: true })
            _timer.Stop();
    }

    private void UpdateTimerInterval()
    {
        if (_timer == null) return;
        var frames = Math.Max(1, _totalFrames);
        var baseMsPerFrame = 10000.0 / frames; // 10 seconds per full loop at 1.0x speed
        var ms = baseMsPerFrame / Math.Max(0.001, _speedMultiplier);
        _timer.Interval = TimeSpan.FromMilliseconds(ms);
    }

    private int CountFrames()
    {
        try
        {
            if (!Directory.Exists(_framesDir)) return 0;
            return Directory.GetFiles(_framesDir, "frame_*.png", SearchOption.TopDirectoryOnly).Length;
        }
        catch
        {
            return 0;
        }
    }

    private void AdvanceFrame()
    {
        if (_totalFrames <= 0)
            return;

        var next = FrameIndex + 1;
        if (next >= _totalFrames)
            next = 0;

        FrameIndex = next;
        // Prefer index-based loading when total frames are known
        LoadFrameIfExists(FrameIndex);
    }

    private void OpenWindow()
    {
        _window = new PlaybackWindow
        {
            DataContext = this
        };
        _window.Closed += (_, _) =>
        {
            StopTimer();
            DisposeBitmap();
        };
        _window.Show();
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_timer == null) return;
        if (_timer.IsEnabled)
            StopTimer();
        else
            StartTimer();
    }

    [RelayCommand]
    private void IncreaseSpeed()
    {
        _speedMultiplier *= 1.5; // +50%
        UpdateTimerInterval();
    }

    [RelayCommand]
    private void DecreaseSpeed()
    {
        _speedMultiplier /= 1.5; // -50%
        // Prevent effectively infinite interval
        if (_speedMultiplier < 0.05) _speedMultiplier = 0.05;
        UpdateTimerInterval();
    }

    [RelayCommand]
    private void NextFrame()
    {
        // Manual step (also useful while paused)
        AdvanceFrame();
    }

    private void LoadFrameOrWrap()
    {
        if (!LoadFrameIfExists(FrameIndex))
        {
            FrameIndex = 0;
            LoadFrameIfExists(FrameIndex);
        }
    }

    private bool LoadFrameIfExists(int index)
    {
        var path = Path.Combine(_framesDir, $"frame_{index:0000}.png");
        if (!File.Exists(path)) return false;

        try
        {
            // Dispose previous bitmap to avoid file locks and leaks.
            DisposeBitmap();
            CurrentFrameBitmap = new Bitmap(path);
            return true;
        }
        catch (Exception)
        {
            // If image fails to load, treat as not available.
            return false;
        }
    }

    private void DisposeBitmap()
    {
        var bmp = CurrentFrameBitmap;
        if (bmp != null)
        {
            CurrentFrameBitmap = null;
            bmp.Dispose();
        }
    }

    [RelayCommand]
    private void ToggleFullscreen()
    {
        if (_window is null) return;

        _window.WindowState = _window.WindowState == WindowState.FullScreen
            ? WindowState.Normal
            : WindowState.FullScreen;
    }
}