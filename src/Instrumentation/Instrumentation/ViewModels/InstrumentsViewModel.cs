using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Instrumentation.Dialogs;
using Instrumentation.Model;
using Instrumentation.Model.Interfaces;
using Instrumentation.Player;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Instrumentation.ViewModels;

public sealed class InstrumentsViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel
{
    private readonly IInstrumentationDataProvider _instrumentation =
            Locator.Current.GetService<IInstrumentationDataProvider>();

    private readonly ISoundPlayer _player =
        Locator.Current.GetService<ISoundPlayer>();

    public string? UrlPathSegment => $"/{Category?.Name}";
    public IScreen HostScreen => Locator.Current.GetService<MainViewModel>();
    public ViewModelActivator Activator { get; } = new();

    [Reactive] public InstrumentCategory Category { get; set; }

    [Reactive] public ObservableCollectionExtended<Instument> Instuments { get; private set; } = new();

    [Reactive] public ObservableCollectionExtended<Sound> Sounds { get; private set; } = new();

    [Reactive] [CanBeNull] public SoundData SelectedSoundData { get; private set; }

    [Reactive] [CanBeNull] public object SelectedInstrument { get; set; }

    [Reactive] [CanBeNull] public object SelectedSound { get; set; }

    [Reactive] public ObservableCollectionExtended<InstrumentImage> InstrumentImages { get; private set; } = new();

    [Reactive] public ObservableCollectionExtended<SoundNoteImage> SoundNoteImages { get; private set; } = new();

    [Reactive] public bool IsLoading { get; private set; }

    [Reactive] public bool CanPlaySound { get; private set; }

    [Reactive] public bool ShowInstrumentImages { get; private set; }

    [Reactive] public bool ShowSoundImages { get; private set; }

    public ReactiveCommand<InstrumentCategory, IEnumerable<Instument>> LoadInstruments { get; }

    public ReactiveCommand<object, IEnumerable<Sound>> LoadSounds { get; }

    public ReactiveCommand<object, SoundData> LoadSoundData { get; }

    public ReactiveCommand<(string audio, string fileName), Unit> PlaySound { get; }

    public ReactiveCommand<Unit, Unit> StopSound { get; }

    public ReactiveCommand<Unit, Unit> StartPlaying { get; }

    public ReactiveCommand<Unit, ImagesCarouselDialogViewModel> ShowImages { get; }

    public ReactiveCommand<object, IEnumerable<InstrumentImage>> LoadInstrumentImages { get; }

    public InstrumentsViewModel Configure(InstrumentCategory category)
    {
        this.Category = category;
        return this;
    }

    public InstrumentsViewModel()
    {
        LoadInstruments = ReactiveCommand.CreateFromTask<InstrumentCategory, IEnumerable<Instument>>(GetInstrumentsAsync);

        LoadSounds = ReactiveCommand.CreateFromTask<object, IEnumerable<Sound>>(GetSoundsAsync);

        LoadSoundData = ReactiveCommand.CreateFromTask<object, SoundData>(GetSoundDataAsync);

        StartPlaying = ReactiveCommand.Create(() => _player.Play(),
            this.WhenAnyValue(x => x.CanPlaySound));
        var canPlaying = this
            .WhenAnyValue(x => x.StartPlaying);
        //StartPlaying = ReactiveCommand.Create(() => _player.Stop());


        PlaySound = ReactiveCommand.Create<(string audio, string fileName), Unit>(x =>
        {
            var (audio, fileName) = x;
            _player.LoadAudio(audio, fileName);
            return Unit.Default;
        });

        // Показываем картинку инструмента некликабельную
        /*
        ShowImages = ReactiveCommand.Create(() =>
        {
            var vm = Locator.Current.GetService<ImagesCarouselDialogViewModel>();

            return SelectedSoundData is not null
                ? vm.Configure(SelectedSoundData.NoteImages, SelectedSoundData.SoundName)
                : null;
        });
        */

        // Показываем картинку инструмента кликабельная
        ShowImages = ReactiveCommand.Create(() =>
        {
            var vm = Locator.Current.GetService<ImagesCarouselDialogViewModel>();

            switch (ShowInstrumentImages)
            {
                case true when InstrumentImages.Count > 0
                               && SelectedInstrument is Instument { IsInstumentGroup: false } instrument:
                    return vm.Configure(InstrumentImages, instrument.Name);
                case true when InstrumentImages.Count > 0
                               && SelectedInstrument is Subinstument subinstrument:
                    return vm.Configure(InstrumentImages, subinstrument.Name);
                default:
                    {
                        if (ShowSoundImages && SoundNoteImages.Count > 0)
                        {
                            var instName = SelectedInstrument switch
                            {
                                Instument { IsInstumentGroup: false } instrument => instrument.Name,
                                Subinstument subinstrument => subinstrument.Name,
                                _ => string.Empty
                            };

                            return vm.Configure(SoundNoteImages, $"{instName}_{SelectedSoundData.SoundName}");
                        }

                        return null;
                    }
            }
        });


        LoadInstrumentImages = ReactiveCommand.CreateFromTask<object, IEnumerable<InstrumentImage>>(GetInstrumentImagesAsync);

        this.WhenActivated(disposables =>
        {
            LoadInstruments.IsExecuting
                .Concat(LoadSounds.IsExecuting)
                .Concat(LoadSoundData.IsExecuting)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IsLoading = x)
                .DisposeWith(disposables);

            // загрузка списка инструментов по выбранной категории
            this.WhenAnyValue(x => x.Category)
                .WhereNotNull()
                .SelectMany(x => LoadInstruments.Execute(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    Sounds.Clear();
                    SelectedSoundData = null;
                    _player.Stop();
                    _player.Clear();
                    InstrumentImages.Clear();
                    SoundNoteImages.Clear();
                    SelectedInstrument = null;
                    Instuments.Load(x);
                })
                .DisposeWith(disposables);

            // загрузка списка звуков по выбранному инструменту
            this.WhenAnyValue(x => x.SelectedInstrument)
                .WhereNotNull()
                .Where(x => x is Instument {IsInstumentGroup: false} or Subinstument)
                .SelectMany(x => LoadSounds.Execute(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    SelectedSound = null;
                    _player.Clear();
                    Sounds.Load(x);
                    InstrumentImages.Clear();
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedInstrument)
                .Where(x => x is null or Instument { IsInstumentGroup: true })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    Sounds.Clear();
                    SelectedSoundData = null;
                    SelectedSound = null;
                    SoundNoteImages.Clear();
                    InstrumentImages.Clear();
                    _player.Clear();
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedInstrument)
                .WhereNotNull()
                .Where(x => x is Instument { IsInstumentGroup: false } or Subinstument)
                .SelectMany(x => LoadInstrumentImages.Execute(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    InstrumentImages.Load(x);
                })
                .DisposeWith(disposables);

            // загрузка данных выбранного звука
            this.WhenAnyValue(x => x.SelectedSound)
                .WhereNotNull()
                .Where(x => x is Sound {IsSoundGroup: false} or Subsound)
                .Do(_ => _player.Stop())
                .Do(_ => SelectedSoundData = null)
                .SelectMany(x => LoadSoundData.Execute(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    SelectedSoundData = x;
                    InstrumentImages.Clear();
                    })
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedSound)
                .Where(x => x is null or Sound { IsSoundGroup: true })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    SelectedSoundData = null;
                    SoundNoteImages.Clear();
                    InstrumentImages.Clear();
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedSound)
                .Where(x => x is Sound { IsSoundGroup: true })
                .Subscribe(_ =>
                {
                    SelectedSoundData = null;
                    _player.Clear();
                    InstrumentImages.Clear();
                })
                .DisposeWith(disposables);

            // загрузка аудиофайла в плеер из данных по звуку
            this.WhenAnyValue(x => x.SelectedSoundData)
                .WhereNotNull()
                .Where(x => !string.IsNullOrWhiteSpace(x.SoundBase64))
                .Select(x => (x.SoundBase64, x.SoundName))
                .SelectMany(x => PlaySound.Execute(x))
                .Subscribe()
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedSoundData)
                .WhereNotNull()
                .Select(x => x.NoteImages)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    SoundNoteImages.Load(x);
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(
                    x => x.InstrumentImages.Count,
                    x => x.SoundNoteImages.Count,
                    x => x.SelectedSound)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    var (instrumentImages, soundNoteImages, selectedSound) = x;

                    if (instrumentImages > 0 && selectedSound is null)
                    {
                        ShowInstrumentImages = selectedSound is null or Sound { IsSoundGroup: true };
                        ShowSoundImages = false;
                    }
                    else if (soundNoteImages > 0)
                    {
                        ShowInstrumentImages = false;
                        ShowSoundImages = true;
                    }
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(
                    x => x.SelectedSound,
                    x => x.SelectedSoundData)
                .Select(x => x.Item1 is not null && x.Item2 is not null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CanPlaySound = x)
                .DisposeWith(disposables);

            LoadInstruments.ThrownExceptions
                .Select(x => $"Ошибка загрузки списка инструментов:{Environment.NewLine}{x.Message}")
                .SelectMany(x =>
                    Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(x, MessageDialogIcon.Error,
                        buttons: MessageDialogButton.Ok)))
                .Subscribe()
                .DisposeWith(disposables);

            LoadSoundData.ThrownExceptions
                .Select(x => $"Ошибка загрузки данных звука:{Environment.NewLine}{x.Message}")
                .SelectMany(x =>
                    Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(x, MessageDialogIcon.Error,
                        buttons: MessageDialogButton.Ok)))
                .Subscribe()
                .DisposeWith(disposables);

            LoadSounds.ThrownExceptions
                .Select(x => $"Ошибка загрузки списка звуков:{Environment.NewLine}{x.Message}")
                .SelectMany(x =>
                    Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(x, MessageDialogIcon.Error,
                        buttons: MessageDialogButton.Ok)))
                .Subscribe()
                .DisposeWith(disposables);

            PlaySound.ThrownExceptions
                .Select(x => $"Ошибка воспроизведения звука:{Environment.NewLine}{x.Message}")
                .SelectMany(x =>
                    Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(x, MessageDialogIcon.Error,
                        buttons: MessageDialogButton.Ok)))
                .Subscribe()
                .DisposeWith(disposables);

            LoadInstrumentImages.ThrownExceptions
                .Select(x => $"Ошибка загрузки изображения инструмента: {Environment.NewLine}{x.Message}")
                .SelectMany(x =>
                    Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(x, MessageDialogIcon.Error,
                        buttons: MessageDialogButton.Ok)))
                .Subscribe()
                .DisposeWith(disposables);

            ShowImages
                .WhereNotNull()
                // нужно для не кликабельной картинки инстурментов
                //.Where(_ => SelectedSoundData is not null || InstrumentImages.Any())
                .SelectMany(x => Interactions.ShowImagesCarousel.Handle(x))
                .Subscribe()
                .DisposeWith(disposables);

            Disposable.Create(() =>
            {
                SelectedSound = null;
                SelectedSoundData = null;
            }).DisposeWith(disposables);
        });
    }

    private async Task<IEnumerable<Instument>> GetInstrumentsAsync(InstrumentCategory category) =>
        await _instrumentation.GetInstrumentsAsync(category).ToListAsync();

    private async Task<IEnumerable<Sound>> GetSoundsAsync(object instrument) =>
        instrument switch
        {
            null => Enumerable.Empty<Sound>(),
            Instument instr when instr.Subinstuments.Count > 0 => Enumerable.Empty<Sound>(),
            Instument instr when instr.Subinstuments.Count == 0
                => await _instrumentation.GetSoundsAsync(instr).ToListAsync(),
            Subinstument subinstr => await _instrumentation.GetSoundsAsync(subinstr).ToListAsync(),
            _ => throw new ArgumentException($"Unknown type of {nameof(instrument)}")
        };

    private async Task<SoundData> GetSoundDataAsync(object sound)
    {
        return sound switch
        {
            null => null,
            Sound {Subsounds.Count: > 0} => null,
            Sound snd when snd.Subsounds.Count == 0 => await _instrumentation.GetSoundDataAsync(snd),
            Subsound ssnd => await _instrumentation.GetSoundDataAsync(ssnd),
            _ => throw new ArgumentException($"Unknown type of {nameof(sound)}")
        };
    }

    private async Task<IEnumerable<InstrumentImage>> GetInstrumentImagesAsync(object instrument)
    {
        return instrument switch
        {
            null => Enumerable.Empty<InstrumentImage>(),
            Instument { Subinstuments.Count: > 0 } => Enumerable.Empty<InstrumentImage>(),
            Instument { IsInstumentGroup: false } inst => await _instrumentation.GetInstrumentImagesAsync(inst).ToListAsync(),
            Subinstument subinst => await _instrumentation.GetSubInstrumentImagesAsync(subinst).ToListAsync(),
            _ => throw new ArgumentException($"Unknown type of {nameof(instrument)}")
        };
    }
}