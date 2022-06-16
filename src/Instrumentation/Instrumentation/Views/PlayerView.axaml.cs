using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Instrumentation.ViewModels;
using ReactiveUI;

namespace Instrumentation.Views;

public partial class PlayerView : ReactiveUserControl<PlayerViewModel>
{
    private Button _playButton;
    private Button _pauseButton;
    private Button _stopButton;
    
    public PlayerView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(
                    x => x.ViewModel.IsPlaying,
                    x => x.ViewModel.IsPaused,
                    x => x.ViewModel.IsStopped)
                .Where(x => x.Item1 == true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    _playButton.IsVisible = false;
                    _pauseButton.IsVisible = true;
                })
                .DisposeWith(disposables);
            
            this.WhenAnyValue(
                    x => x.ViewModel.IsPlaying,
                    x => x.ViewModel.IsPaused,
                    x => x.ViewModel.IsStopped)
                .Where(x => x.Item1 == false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    _playButton.IsVisible = true;
                    _pauseButton.IsVisible = false;
                })
                .DisposeWith(disposables);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _playButton = this.FindControl<Button>("PlayButton");
        _pauseButton = this.FindControl<Button>("PauseButton");
        _stopButton = this.FindControl<Button>("StopButton");
    }
}