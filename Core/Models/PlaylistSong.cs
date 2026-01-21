using Riffle.Core.Utilities;

namespace Riffle.Core.Models;

public class PlaylistSong
{
    private Playlist _playlist = null!;

    public Playlist Playlist
    {
        get => _playlist;
        private set
        {
            _playlist = value;
            PlaylistId = value.Id;
        }
    }
    public Guid PlaylistId { get; private set; }

    private Song _song = null!;
    public Song Song
    {
        get => _song;
        private set
        {
            _song = value;
            SongId = value.Id;
        }
    }

    public Guid SongId { get; private set; }

    public DateTime DateAdded { get; private set; }

    public string HowLongAgo => DateAdded.ToFriendlyAge();

    public PlaylistSong()
    {
        
    }

    public PlaylistSong(Song song, Playlist playlist)
    {
        Song = song;
        Playlist = playlist;
        DateAdded = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PlaylistSong otherSong) return false;
        
        return SongId.Equals(otherSong.SongId) && PlaylistId.Equals(otherSong.PlaylistId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SongId, PlaylistId);
    }
}