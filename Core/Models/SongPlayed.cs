using Riffle.Core.Utilities;

namespace Riffle.Core.Models;

public class SongPlayed
{
    public Song Song { get; private set; }
    public Guid SongId { get; private set; }
    public DateTime PlayedAt { get; private set; }
    public Playlist PlayedFrom { get; private set; }
    public Guid? PlayedFromId { get; private set; }
    public string? PlayedFromName { get; set; }
    public Guid Id { get; private set; }
    
    public string HowLongAgo => PlayedAt.ToFriendlyAge();

    public SongPlayed(Song song, DateTime playedAt, Playlist playedFrom)
    {
        Song = song;
        SongId = song.Id;
        PlayedAt = playedAt;
        PlayedFrom = playedFrom;
        PlayedFromId = playedFrom.Id;
        PlayedFromName = playedFrom.Name;
        Id = Guid.NewGuid();
    }

    public SongPlayed()
    {
        
    }
    
    public static implicit operator Song(SongPlayed songPlayed) => songPlayed.Song;
}