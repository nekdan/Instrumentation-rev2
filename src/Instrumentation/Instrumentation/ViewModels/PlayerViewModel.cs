using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Instrumentation.Player;
using LazyCache;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Instrumentation.ViewModels;

public sealed class PlayerViewModel: ViewModelBase, IActivatableViewModel
{
    private readonly ISoundPlayer _player = Locator.Current.GetService<ISoundPlayer>();
    private readonly IAppCache _cache = Locator.Current.GetService<IAppCache>();

    private const string VOLUME_KEY = "soundVolume";
    private double? _currentPosition;

    public ViewModelActivator Activator { get; } = new();
    
    [Reactive]
    public TimeSpan? SoundDuration { get; private set; }
    
    [Reactive]
    public TimeSpan? SoundPosititon { get; private set; }

    [Reactive] public double? SoundDurationTotalSeconds { get; private set; } = 0;
    
    [Reactive] public double? SoundPositionTotalSeconds { get; private set; } = 0;
    
    [Reactive] public double SoundVolume { get; set; }
    
    [Reactive] public bool IsPlaying { get; private set; }
    
    [Reactive] public bool IsPaused { get; private set; }
    
    [Reactive] public bool IsStopped { get; private set; }
    
    [Reactive] public bool IsVolumeMuted { get; private set; }

    [Reactive] public bool CanChangeVolume { get; private set; }
    
    [Reactive] public string SoundName { get; private set; }
    
    [Reactive] public bool AudioIsLoaded { get; private set; }

    public double MaxVolume => _player.MaxVolume;

    public double MinVolume => _player.MinVolume;
    
    public ReactiveCommand<Unit, Unit> Play { get; }
    
    public ReactiveCommand<Unit, Unit> Pause { get; }
    
    public ReactiveCommand<Unit, Unit> Stop { get; }
    
    public ReactiveCommand<Unit, Unit> Mute { get; }
    
    public ReactiveCommand<Unit, Unit> Unmute { get; }

    [Reactive]
    public string Timer { get; private set; }

    public PlayerViewModel()
    {
        Play = ReactiveCommand.Create(() => _player.Play());
        Pause = ReactiveCommand.Create(() => _player.Pause());
        Stop = ReactiveCommand.Create(() => _player.Stop());
        Mute = ReactiveCommand.Create(MuteUnmuteSound);
        Unmute = ReactiveCommand.Create(MuteUnmuteSound);

        this.WhenActivated(disposables =>
        {
            SoundVolume = _player.CurrentVolume;

            _player.AudioIsLoaded
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => AudioIsLoaded = x)
                .DisposeWith(disposables);

            _player.IsPlaying
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IsPlaying = x)
                .DisposeWith(disposables);

            _player.IsPaused
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IsPaused = x)
                .DisposeWith(disposables);

            _player.IsStopped
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IsStopped = x)
                .DisposeWith(disposables);

            _player.SoundDuration
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SoundDurationTotalSeconds = x)
                .DisposeWith(disposables);

            _player.SoundPosition
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    _currentPosition = x;
                    SoundPositionTotalSeconds = x;
                })
                .DisposeWith(disposables);
            
            _player.PlayingFileName
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SoundName = x)
                .DisposeWith(disposables);
            
            this.WhenAnyValue(x => x.SoundDurationTotalSeconds)
                .DistinctUntilChanged()
                .Select(x => TimeSpan.FromSeconds(x ?? 0))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SoundDuration = x)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SoundPositionTotalSeconds)
                .Select(x => TimeSpan.FromSeconds(x ?? 0))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SoundPosititon = x)
                .DisposeWith(disposables);
            
            this.WhenAnyValue(x => x.SoundVolume)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => _player.SetVolume(x))
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SoundPositionTotalSeconds)
                .DistinctUntilChanged()
                .Where(_ => _currentPosition is not null)
                .Where(x => x != _currentPosition)
                .Subscribe(x => _player.SetPosition(x.Value))
                .DisposeWith(disposables);

            this.WhenAnyValue(
                    x => x.SoundDuration,
                    x => x.SoundPosititon)
                .Where(x => x.Item1 != null && x.Item2 != null)
                .Select(x => $"{x.Item2 ?? TimeSpan.Zero:mm\\:ss}/{x.Item1 ?? TimeSpan.Zero:mm\\:ss}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Timer = x)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.IsVolumeMuted)
                .Select(x => !x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CanChangeVolume = x)
                .DisposeWith(disposables);

            Mute
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    IsVolumeMuted = true;
                    SoundVolume = 0;
                })
                .DisposeWith(disposables);

            Unmute
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    IsVolumeMuted = false;
                    SoundVolume = _cache.Get<double>(VOLUME_KEY);
                })
                .DisposeWith(disposables);
        });
    }

    private void MuteUnmuteSound()
    {
        switch (IsVolumeMuted)
        {
            case false:
                _cache.Add(VOLUME_KEY, SoundVolume);
                _player.SetVolume(0.0);
                break;
            case true:
                _player.SetVolume(_cache.Get<double>(VOLUME_KEY));
                break;
        }
    }
}