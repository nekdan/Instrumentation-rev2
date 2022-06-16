using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LazyCache;
using ManagedBass;
using Splat;

namespace Instrumentation.Player;

public class SoundPlayer : ISoundPlayer
{
    private const string SOUND_NAME_CACHE_KEY = "PLAYER_soundname";
    private const string SOUND_DURATION = "PLAYER_duration";
    private readonly System.Threading.Timer _bassStatusTimer;
    private readonly IAppCache _cache = Locator.Current.GetService<IAppCache>();
    private readonly Subject<int> _error = new();
    private readonly Subject<bool> _isPaused = new();
    private readonly Subject<bool> _isPlaying = new();
    private readonly Subject<bool> _isStopped = new();
    private readonly Subject<double> _soundDuration = new();
    private readonly Subject<string> _soundName = new();
    private readonly Subject<double> _soundPosition = new();
    private readonly Subject<bool> _audioIsLoaded = new();

    private readonly string _tmpFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmpaudio");
    private double? _volume;
    private int? currentStreamHandle;

    public SoundPlayer()
    {
        _bassStatusTimer = new System.Threading.Timer(
            RefreshPosition,
            null,
            0, 
            (int)TimeSpan.FromSeconds(0.9).TotalMilliseconds);

        Bass.Free();
        Bass.Init();
    }

    public IObservable<int> Error => _error.AsObservable();

    public double MaxVolume => 1;
    public double MinVolume => 0;
    public double CurrentVolume => Bass.Volume;

    public IObservable<string> PlayingFileName => _soundName.AsObservable();
    public IObservable<double> SoundDuration => _soundDuration.AsObservable();
    public IObservable<double> SoundPosition => _soundPosition.AsObservable();
    public IObservable<bool> IsPlaying => _isPlaying.AsObservable();
    public IObservable<bool> IsPaused => _isPaused.AsObservable();
    public IObservable<bool> IsStopped => _isStopped.AsObservable();
    public IObservable<bool> AudioIsLoaded => _audioIsLoaded.AsObservable();

    public void LoadAudio(string audioBase64, string fileName)
    {
        if (currentStreamHandle is not null)
        {
            Bass.ChannelStop(currentStreamHandle.Value);
        }

        if (!Directory.Exists(_tmpFolder))
            Directory.CreateDirectory(_tmpFolder);

        ClearTmpFolder();

        var tmpFileName = Path.Combine(_tmpFolder, $"{Guid.NewGuid().ToString()}.wav");

        if (File.Exists(tmpFileName))
            File.Delete(tmpFileName);
        
        Bass.Free();
        Bass.Init();
        var bytes = Convert.FromBase64String(audioBase64);
        using var ms = new MemoryStream(bytes);
        using var fs = new FileStream(tmpFileName, FileMode.CreateNew, FileAccess.ReadWrite);
        ms.WriteTo(fs);
        fs.Flush();

        currentStreamHandle = Bass.CreateStream(tmpFileName,
            Flags: BassFlags.Default | BassFlags.Prescan);

        if (_volume is not null) SetVolume(_volume.Value);

        _cache.Add(SOUND_NAME_CACHE_KEY, fileName);
        _cache.Add(SOUND_DURATION, Bass.ChannelBytes2Seconds(
            currentStreamHandle.Value, Bass.ChannelGetLength(currentStreamHandle.Value)));
        _soundName.OnNext(_cache.Get<string>(SOUND_NAME_CACHE_KEY));
        _audioIsLoaded.OnNext(true);
    }

    public void Play()
    {
        if (currentStreamHandle is null) return;

        Bass.ChannelPlay(currentStreamHandle.Value);
        _isPlaying.OnNext(true);
        _isPaused.OnNext(false);
        _isStopped.OnNext(false);

        _soundName.OnNext(_cache.Get<string>(SOUND_NAME_CACHE_KEY));
    }

    public void Pause()
    {
        if (currentStreamHandle is null) return;

        Bass.ChannelPause(currentStreamHandle.Value);

        _isPlaying.OnNext(false);
        _isPaused.OnNext(true);
        _isStopped.OnNext(false);
    }

    public void Stop()
    {
        if (currentStreamHandle is null) return;

        Bass.ChannelStop(currentStreamHandle.Value);
        SetPosition(0);

        _isPlaying.OnNext(false);
        _isPaused.OnNext(false);
        _isStopped.OnNext(true);

        //_soundName.OnNext(string.Empty);
    }

    public void Clear()
    {
        if (currentStreamHandle is not null)
            Bass.ChannelStop(currentStreamHandle.Value);

        Bass.Free();
        currentStreamHandle = null;

        _soundName.OnNext(string.Empty);
    }

    public void SetVolume(double volume)
    {
        _volume = volume;

        if (currentStreamHandle is null)
            return;

        if (volume > 1 && volume < 0)
            throw new ArgumentException(nameof(volume));

        Bass.ChannelSetAttribute(currentStreamHandle.Value, ChannelAttribute.Volume, volume);
    }

    public void SetPosition(double seconds)
    {
        if (currentStreamHandle is not null)
            Bass.ChannelSetPosition(
                currentStreamHandle.Value, Bass.ChannelSeconds2Bytes(currentStreamHandle.Value, seconds));
    }

    public void Dispose()
    {
        _soundName?.Dispose();
        _soundDuration?.Dispose();
        _soundPosition?.Dispose();
        _isPlaying?.Dispose();
        _isPaused?.Dispose();
        _isStopped?.Dispose();
        _error?.Dispose();
        _bassStatusTimer?.Dispose();

        Bass.Free();

        DeleteTmpFolder();
    }

    private void ClearTmpFolder()
    {
        if (!Directory.Exists(_tmpFolder))
            return;

        foreach (var file in Directory.EnumerateFiles(_tmpFolder))
            File.Delete(file);
    }

    private void DeleteTmpFolder()
    {
        if (!Directory.Exists(_tmpFolder))
            return;

        foreach (var file in Directory.EnumerateFiles(_tmpFolder))
            File.Delete(file);

        Directory.Delete(_tmpFolder);
    }
    
    private async void RefreshPosition(object o)
    {
        if (currentStreamHandle is null) return;
        
        var position = await Task.Run(() => Bass.ChannelBytes2Seconds(
            currentStreamHandle.Value, Bass.ChannelGetPosition(currentStreamHandle.Value)));

        var duration = _cache.Get<double>(SOUND_DURATION);
        _soundDuration.OnNext(duration);
        _soundPosition.OnNext(position);
        CheckEndReached(position: position, duration: duration);
    }

    private void CheckEndReached(double position, double duration)
    {
        if (currentStreamHandle is null
            || position != duration)
            return;

        _isStopped.OnNext(true);
        _isPaused.OnNext(false);
        _isPlaying.OnNext(false);
    }
}