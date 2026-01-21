namespace Riffle.Core.Models;

public class SongPlayed
{
    public PlaylistSong Song { get; }
    public DateTime PlayedAt { get; }
    public Playlist? PlayedFrom { get; }

    public SongPlayed(PlaylistSong song, DateTime playedAt, Playlist? playedFrom)
    {
        Song = song;
        PlayedAt = playedAt;
        PlayedFrom = playedFrom;
    }
    
    public static implicit operator PlaylistSong(SongPlayed songPlayed) => songPlayed.Song;
}