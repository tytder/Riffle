namespace Riffle.Core.Models;

public class SongPlayed
{
    public Song Song { get; }
    public DateTime PlayedAt { get; }

    public SongPlayed(Song song, DateTime playedAt)
    {
        Song = song;
        PlayedAt = playedAt;
    }
}