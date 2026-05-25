using Riffle.Core.Models;
using Riffle.Core.Utilities;

public class SongPlayed
{
    // Relationship FKs
    public Guid? SongId { get; private set; }
    public Guid? PlaylistId { get; private set; }
    public Guid? PlaylistSongId { get; private set; }

    public Song? Song { get; private set; }
    public Playlist? Playlist { get; private set; }
    public PlaylistSong? PlaylistSong { get; private set; }

    public DateTime PlayedAt { get; private set; }

    // Snapshots
    public string SongName { get; private set; } = null!;
    public string? ArtistName { get; private set; }
    public string? PlayedFromName { get; private set; }

    public Guid Id { get; private set; }
    public string HowLongAgo => PlayedAt.ToFriendlyAge();

    public SongPlayed(PlaylistSong playlistSong, DateTime playedAt)
    {
        PlaylistSong = playlistSong;
        PlaylistSongId = playlistSong.Id;

        Song = playlistSong.Song;
        SongId = playlistSong.Song.Id;

        Playlist = playlistSong.Playlist;
        PlaylistId = playlistSong.Playlist.Id;

        PlayedAt = playedAt;

        // snapshots at the time of play
        SongName = playlistSong.Song.Title;
        ArtistName = playlistSong.Song.Artist;
        PlayedFromName = playlistSong.Playlist.Name;
    }

    public SongPlayed() { }

    public bool TryMarkPlaylistDeleted()
    {
        if (PlayedFromName != null &&
            !PlayedFromName.EndsWith(" (deleted)", StringComparison.Ordinal))
        {
            PlayedFromName += " (deleted)";
            return true;
        }

        return false;
    }
    public bool TryMarkSongDeleted()
    {
        if (!SongName.EndsWith(" (deleted)", StringComparison.Ordinal))
        {
            SongName += " (deleted)";
            return true;
        }

        return false;
    }
    
    public static implicit operator Song?(SongPlayed songPlayed) => songPlayed.PlaylistSong?.Song ?? songPlayed.Song; 
}