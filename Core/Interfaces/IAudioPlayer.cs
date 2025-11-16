using Riffle.Core.Models;

namespace Riffle.Core.Interfaces
{
    public interface IAudioPlayer
    {
        void Play(Song song);
        void TogglePlaying();
        void StopAll();
        bool IsPlaying { get;  }
        bool HasTrackLoaded { get; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        float Volume { get; }
        void SetVolume(float volume);
        string SongTitle { get; }
        void Seek(TimeSpan fromSeconds);
        event EventHandler<TrackEventArgs> TrackLoaded;
        event EventHandler TrackEnded;
        event EventHandler StopAllCalled;
        event EventHandler<PlayingStateEventArgs> PlayingStateChanged;
    }
}