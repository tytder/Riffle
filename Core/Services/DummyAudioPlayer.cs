using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;

namespace Riffle.Core.Services;

public class DummyAudioPlayer : IAudioPlayer
{
    public void Play(Song song)
    {
        // no op
    }

    public void TogglePlaying()
    {
        // no op
    }

    public void StopAll()
    {
        // no op
    }

    public bool IsPlaying { get; }
    public bool HasTrackLoaded { get; }
    public TimeSpan CurrentTime { get; }
    public TimeSpan TotalTime { get; }
    public float Volume { get; }
    public void SetVolume(float volume)
    {
        // no op
    }

    public string SongTitle { get; }
    public void Seek(TimeSpan fromSeconds)
    {
        // no op
    }

    public event EventHandler<TrackEventArgs>? TrackLoaded;
    public event EventHandler? TrackEnded;
    public event EventHandler? StopAllCalled;
    public event EventHandler<PlayingStateEventArgs>? PlayingStateChanged;

    public void Dispose()
    {
        // no op
    }
}