namespace Riffle.Core.Audio
{
    public interface IAudioPlayer
    {
        void Play(Song song);
        void Pause();
        void Resume();
        void Stop();
        bool IsPlaying { get; set; }
        bool HasTrackLoaded { get; set; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        float Volume { get; set; }
        string SongTitle { get; }
        event EventHandler<Song>? TrackLoaded;
        void Seek(TimeSpan fromSeconds);
        event EventHandler? TrackEnded;
    }
}