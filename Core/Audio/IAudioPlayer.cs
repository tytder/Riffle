namespace Riffle.Core.Audio
{
    public interface IAudioPlayer
    {
        void Play(string filePath);
        void Pause();
        void Resume();
        void Stop();
        bool IsPlaying { get; set; }
        bool HasTrackLoaded { get; set; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        event EventHandler? TrackLoaded;
        void Seek(TimeSpan fromSeconds);
    }
}