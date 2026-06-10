using Riffle.Core.Utilities;

namespace Riffle.Core.Models;

public class PlaylistSong
{
    public Playlist Playlist { get; private set; } = null!;

    public Guid PlaylistId { get; private set; }

    public Song Song { get; private set; } = null!;

    public Guid SongId { get; private set; }

    public DateTime DateAdded { get; private set; }

    public string HowLongAgo => DateAdded.ToFriendlyAge();
    
    public Guid Id { get; private set; }

    public PlaylistSong()
    {
        
    }

    public PlaylistSong(Song song, Playlist playlist)
    {
        Song = song;
        SongId = song.Id;
        Playlist = playlist;
        PlaylistId = playlist.Id;
        DateAdded = DateTime.UtcNow;
    }

    public PlaylistSong(PlaylistSong original, Playlist newPlaylist)
    {
        Song = original.Song;
        SongId = original.Song.Id;
        Playlist = newPlaylist;
        PlaylistId = newPlaylist.Id;
        DateAdded = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PlaylistSong otherSong) return false;
        
        return SongId.Equals(otherSong.SongId) && PlaylistId.Equals(otherSong.PlaylistId);
    }

    public override int GetHashCode()
    {
        return PlaylistId.GetHashCode();
    }
}