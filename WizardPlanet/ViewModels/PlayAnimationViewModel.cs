// ViewModels/PlayAnimationViewModel.cs

using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WizardPlanet.Models;
using WizardPlanet.Views;

namespace WizardPlanet.ViewModels;

public partial class PlayAnimationViewModel : ViewModelBase
{
    public GlobeAnimation Animation { get; }
    private readonly string _framesDir;

    [ObservableProperty] private Bitmap? _currentBitmap;
    [ObservableProperty] private double _angleDegrees;

    private PlaybackWindow? _window;
    private DispatcherTimer? _timer;
    private double _speedMultiplier = 1.0; // 1.0x = 10s per full rotation (36 deg/s baseline)
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _lastTickTime;

    public double SpeedMultiplier => _speedMultiplier;

    public PlayAnimationViewModel(GlobeAnimation animation)
    {
        Animation = animation;
        _framesDir = Path.Combine("Animations", animation.DirectoryPath ?? string.Empty);

        LoadOriginal();

        AngleDegrees = 0;
        InitTimer();
        StartTimer();

        OpenWindow();
    }

    private void LoadOriginal()
    {
        try
        {
            var path = Path.Combine(_framesDir, "frame_0000.png");
            if (File.Exists(path))
            {
                CurrentBitmap?.Dispose();
                CurrentBitmap = new Bitmap(path);
            }
        }
        catch
        {
            Console.Error.WriteLine("Failed to load globe image");
        }
    }

    private void InitTimer()
    {
        _timer = new DispatcherTimer
        {
            // 30 frames per second target
            Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0)
        };
        _timer.Tick += OnTick;

        _stopwatch.Restart();
        _lastTickTime = TimeSpan.Zero;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = _stopwatch.Elapsed;
        var dt = now - _lastTickTime;
        _lastTickTime = now;

        // Baseline angular velocity: 360 degrees / 10 seconds = 36 deg/s
        const double baseDegPerSecond = 360.0 / 10.0;
        var deg = baseDegPerSecond * _speedMultiplier * dt.TotalSeconds;

        AngleDegrees = (AngleDegrees + deg) % 360.0;
        if (AngleDegrees < 0) AngleDegrees += 360.0;
    }

    private void StartTimer()
    {
        if (_timer is { IsEnabled: false })
        {
            _lastTickTime = _stopwatch.Elapsed;
            _timer.Start();
        }
    }

    private void StopTimer()
    {
        if (_timer is { IsEnabled: true })
            _timer.Stop();
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
            _window = null; // allow reopening after close
        };
        _window.Show();
    }

    [RelayCommand]
    private void ReopenPlaybackWindow()
    {
        if (_window is not null)
        {
            _window.Close();
            return;
        }

        OpenWindow();
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_timer == null) return;
        if (_timer.IsEnabled)
            StopTimer();
        else
        {
            // Reset tick origin to avoid a big dt on resume
            _lastTickTime = _stopwatch.Elapsed;
            StartTimer();
        }
    }

    [RelayCommand]
    private void IncreaseSpeed()
    {
        _speedMultiplier *= 1.5; // +50%
    }

    [RelayCommand]
    private void DecreaseSpeed()
    {
        _speedMultiplier /= 1.5; // -50%
        if (_speedMultiplier < 0.05) _speedMultiplier = 0.05;
    }

    [RelayCommand]
    private void ToggleFullscreen()
    {
        if (_window is null) return;
        _window.WindowState = _window.WindowState == WindowState.FullScreen
            ? WindowState.Normal
            : WindowState.FullScreen;
    }

    private void DisposeBitmap()
    {
        var bmp = CurrentBitmap;
        if (bmp != null)
        {
            CurrentBitmap = null;
            bmp.Dispose();
        }
    }
}