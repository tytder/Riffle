namespace Riffle.Core.Audio
{
    public interface IAudioPlayer
    {
        void Play(Song song);
        void Pause();
        void Resume();
        void StopAll();
        bool IsPlaying { get; set; }
        bool HasTrackLoaded { get; set; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        float Volume { get; set; }
        string SongTitle { get; }
        void Seek(TimeSpan fromSeconds);
        event Action<Song> TrackLoaded;
        event Action TrackEnded;
        event Action StopAllCalled;
    }
}