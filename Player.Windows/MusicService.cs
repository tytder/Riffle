using System.Collections.Generic;
using System.Linq;
using Data;
using Microsoft.EntityFrameworkCore;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class MusicService
{
    private readonly DbContextOptions<MusicDbContext> _options;

    public MusicService(DbContextOptions<MusicDbContext> options)
    {
        _options = options;
    }

    /// <summary>
    /// Add a new song. If playlist == null, it only goes into Songs (All Songs).
    /// If playlist != null, it goes into Songs and into the given Playlist.
    /// </summary>
    public Song AddSong(Song song, Playlist? playlist = null)
    {
        using var db = new MusicDbContext(_options);

        // Always add to Songs table
        db.Songs.Add(song);

        // If a playlist is provided, link the song to it
        if (playlist != null)
        {
            var dbPlaylist = db.Playlists
                .Include(p => p.PlaylistItems)
                .First(p => p.Id == playlist.Id);

            dbPlaylist.PlaylistItems.Add(song);
        }

        db.SaveChanges();
        return song;
    }

    public List<Song> GetAllSongs()
    {
        using var db = new MusicDbContext(_options);

        return db.Songs.ToList();
    }
    
    public List<Song> GetSongsForPlaylist(Playlist playlist)
    {
        using var db = new MusicDbContext(_options);

        var dbPlaylist = db.Playlists
            .Include(p => p.PlaylistItems)
            .First(p => p.Id == playlist.Id);

        return dbPlaylist.PlaylistItems.ToList();
    }

    public Playlist CreatePlaylist(string name)
    {
        using var db = new MusicDbContext(_options);
        var playlist = new Playlist(name);
        db.Playlists.Add(playlist);
        db.SaveChanges();
        return playlist;
    }

    public List<Playlist> GetAllPlaylists()
    {
        using var db = new MusicDbContext(_options);
        return db.Playlists.Include(p => p.PlaylistItems).ToList();
    }

    public void DeletePlaylist(Playlist playlist)
    {
        using var db = new MusicDbContext(_options);
        var dbPlaylist = db.Playlists.Include(p => p.PlaylistItems).FirstOrDefault(p => p.Id == playlist.Id);
        if (dbPlaylist != null)
        {
            // Option 1: If join table uses explicit entity, remove its rows first.
            // Option 2: If implicit many-to-many, remove relationships:
            dbPlaylist.PlaylistItems.Clear();
            db.Playlists.Remove(dbPlaylist);
            db.SaveChanges();
        }
    }
}
