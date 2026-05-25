using Microsoft.EntityFrameworkCore;
using Riffle.Core.Models;

namespace Riffle.Data;

public class FakeLibraryManager : ILibraryManager
{
    public Guid AllSongsPlaylistId { get; private set; }
    private List<Playlist> _playlists = new();
    private List<Song> _songs = new();
    private List<PlaylistSong> _playlistSongs = new();
    private List<SongPlayed> _songHistory;
    public bool IsInitialized { get; private set; }

    public async void Initialize()
    {
        if (IsInitialized) return;
        await EnsureAllSongsPlaylistAsync();
        IsInitialized = true;
    }

    public PlaylistSong AddNewSongToPlaylist(Song newSong, Guid playlistToAddId)
    {
        // Always add song (and link) to All Songs first
        var allSongs = _playlists.First(p => p.Id == AllSongsPlaylistId);
        var allSongsLink = AddSong(newSong, allSongs);

        // If target playlist is All Songs, we’re done
        if (playlistToAddId == AllSongsPlaylistId)
            return allSongsLink;

        // Otherwise add another PlaylistSong row reusing the same Song
        Playlist playlistToAdd = _playlists.First(p => p.Id == playlistToAddId);
        
        var playlistLink = new PlaylistSong(allSongsLink.Song, playlistToAdd);
        playlistToAdd.PlaylistItems.Add(playlistLink);
        _playlistSongs.Add(playlistLink);

        return playlistLink;
    }

    public PlaylistSong AddExistingSongToPlaylist(Guid songId, Guid playlistToAddId)
    {
        var playlist = _playlists.First(p => p.Id == playlistToAddId);
        var song     = _songs.First(s => s.Id == songId);

        var playlistSong = new PlaylistSong(song, playlist);
        playlist.PlaylistItems.Add(playlistSong);
        _playlistSongs.Add(playlistSong);

        return playlistSong;
    }

    public IEnumerable<PlaylistSong> GetAllSongsPlaylist()
    {
        var allSongsPlaylist = _playlists
            //.Include(playlist => playlist.PlaylistItems)
            .FirstOrDefault(p => p.Id == AllSongsPlaylistId);
        allSongsPlaylist ??= EnsureAllSongsPlaylistAsync().Result;
        return allSongsPlaylist.PlaylistItems;
    }

    public void MarkLastPlayedPlaylist(Guid playlistId)
    {
        throw new NotImplementedException();
    }

    public Task<Playlist> EnsureAllSongsPlaylistAsync()
    {
        try
        {
            const string AllSongsName = "All Songs";
            var playlist = _playlists.FirstOrDefault(p => p.Name == AllSongsName);

            if (playlist == null)
            {
                playlist = new Playlist(AllSongsName, true);
                _playlists.Add(playlist);
            }

            AllSongsPlaylistId = playlist.Id;
            return Task.FromResult(playlist);
        }
        catch (Exception exception)
        {
            return Task.FromException<Playlist>(exception);
        }
    }
    
    /// <summary>
    /// Add a new song. If playlist == null, it only goes into Songs (All Songs).
    /// If playlist != null, it goes into Songs and into the given Playlist.
    /// </summary>
    public PlaylistSong AddSong(Song song, Playlist playlist)
    {
        // Always add to Songs table
        _songs.Add(song);

        var playlistSong = new PlaylistSong(song, playlist);
        
        // If a playlist is provided, link the song to it
        playlist.PlaylistItems.Add(playlistSong);

        return playlistSong;
    }
    
    public IEnumerable<PlaylistSong> GetSongsForPlaylist(Guid playlistId)
    {
        var dbPlaylist = _playlists
            //.Include(p => p.PlaylistItems)
            //.ThenInclude(ps => ps.Song)
            .First(p => p.Id == playlistId);

        return dbPlaylist.PlaylistItems.ToList();
    }
    
    public Task<List<PlaylistSong>> GetSongsForPlaylistAsync(Playlist playlist)
    {
        try
        {
            var dbPlaylist =  _playlists
                //.Include(p => p.PlaylistItems)
                //.ThenInclude(ps => ps.Song)
                .First(p => p.Id == playlist.Id);

            return Task.FromResult(dbPlaylist.PlaylistItems.ToList());
        }
        catch (Exception exception)
        {
            return Task.FromException<List<PlaylistSong>>(exception);
        }
    }

    public Playlist CreatePlaylist(string name)
    {
        var playlist = new Playlist(name);
        _playlists.Add(playlist);
        return playlist;
    }

    public IEnumerable<Playlist> GetAllPlaylists()
    {
        return _playlists
            //.Include(p => p.PlaylistItems)
            .OrderByDescending(p => p.IsAllSongs)
            .ThenBy(p => p.Name)
            .ToList();
    }

    public void DeletePlaylist(Guid playlistId)
    {
        var dbPlaylist = _playlists
            //.Include(p => p.PlaylistItems)
            .FirstOrDefault(p => p.Id == playlistId);

        if (dbPlaylist == null) return;

        // mark history entries
        var history = _songHistory // whatever DbSet<SongPlayed> is called
            .Where(h => h.PlaylistId == dbPlaylist.Id)
            .ToList();

        foreach (var h in history)
        {
            h.TryMarkPlaylistDeleted();
        }

        dbPlaylist.PlaylistItems.Clear();
        _playlists.Remove(dbPlaylist);
        //_db.SaveChanges();
    }
}