namespace Riffle.Core.Models;

public class SongPlayed
{
    public Song Song { get; }
    public DateTime PlayedAt { get; }
    public Playlist? PlayedFrom { get; }

    public SongPlayed(Song song, DateTime playedAt, Playlist? playedFrom)
    {
        Song = song;
        PlayedAt = playedAt;
        PlayedFrom = playedFrom;
    }
    
    public static implicit operator Song(SongPlayed songPlayed) =>  songPlayed.Song;
}