using Microsoft.EntityFrameworkCore;
using Riffle.Core.Models;
using Riffle.Data;

namespace Riffle.Player.Windows.Services;

#nullable enable
internal class MusicService
{
    private readonly MusicDbContext _db;

    public MusicService(MusicDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Add a new song. If playlist == null, it only goes into Songs (All Songs).
    /// If playlist != null, it goes into Songs and into the given Playlist.
    /// </summary>
    public PlaylistSong AddSong(Song song, Playlist playlist)
    {
        // Always add to Songs table
        _db.Songs.Add(song);

        var playlistSong = new PlaylistSong(song, playlist);
        
        // If a playlist is provided, link the song to it
        var dbPlaylist = _db.Playlists
            .Include(p => p.PlaylistItems)
                .ThenInclude(ps => ps.Song)
            .First(p => p.Id == playlist.Id);

        dbPlaylist.PlaylistItems.Add(playlistSong);

        _db.SaveChanges();
        return playlistSong;
    }
    
    public List<PlaylistSong> GetSongsForPlaylist(Guid playlistId)
    {
        var dbPlaylist = _db.Playlists
            .Include(p => p.PlaylistItems)
                .ThenInclude(ps => ps.Song)
            .First(p => p.Id == playlistId);

        return dbPlaylist.PlaylistItems.ToList();
    }
    
    public async Task<List<PlaylistSong>> GetSongsForPlaylistAsync(Playlist playlist)
    {
        var dbPlaylist = await _db.Playlists
            .Include(p => p.PlaylistItems)
                .ThenInclude(ps => ps.Song)
            .FirstAsync(p => p.Id == playlist.Id);

        return dbPlaylist.PlaylistItems.ToList();
    }

    public Playlist CreatePlaylist(string name)
    {
        var playlist = new Playlist(name);
        _db.Playlists.Add(playlist);
        _db.SaveChanges();
        return playlist;
    }

    public List<Playlist> GetAllPlaylists()
    {
        return _db.Playlists
            .Include(p => p.PlaylistItems)
            .OrderByDescending(p => p.IsAllSongs)
            .ThenBy(p => p.Name)
            .ToList();
    }

    public void DeletePlaylist(Guid playlistId)
    {
        var dbPlaylist = _db.Playlists
            .Include(p => p.PlaylistItems)
            .FirstOrDefault(p => p.Id == playlistId);

        if (dbPlaylist == null) return;

        // mark history entries
        var history = _db.SongHistory // whatever DbSet<SongPlayed> is called
            .Where(h => h.PlayedFromId == dbPlaylist.Id)
            .ToList();

        foreach (var h in history)
        {
            if (h.PlayedFromName != null &&
                !h.PlayedFromName.EndsWith(" (deleted)", StringComparison.Ordinal))
            {
                h.PlayedFromName += " (deleted)";
            }
        }

        dbPlaylist.PlaylistItems.Clear();
        _db.Playlists.Remove(dbPlaylist);
        _db.SaveChanges();
    }
}
