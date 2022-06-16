using System;

namespace Instrumentation.Player;

public interface ISoundPlayer : IDisposable
{
    double MaxVolume { get; }
    double MinVolume { get; }
    double CurrentVolume { get; }
    void LoadAudio(string audioBase64, string fileName);
    void Play();
    void Pause();
    void Stop();
    void Clear();
    void SetVolume(double volume);
    void SetPosition(double seconds);
    IObservable<string> PlayingFileName { get; }
    IObservable<double> SoundDuration { get; }
    IObservable<double> SoundPosition { get; }
    IObservable<bool> IsPlaying { get; }
    IObservable<bool> IsPaused { get; }
    IObservable<bool> IsStopped { get; }
    IObservable<bool> AudioIsLoaded { get; }
}