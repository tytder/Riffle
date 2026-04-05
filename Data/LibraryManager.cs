using Microsoft.EntityFrameworkCore;
using Riffle.Core.Models;
using Riffle.Player.Windows.Services;

namespace Riffle.Data;

public class LibraryManager : ILibraryManager
{
    private readonly MusicDbContext _db;
    private readonly MusicService _musicService;
    public Guid AllSongsPlaylistId { get; private set; }

    public LibraryManager(MusicDbContext db)
    {
        _db = db;
        _musicService = new MusicService(db);
    }

    public async void Initialize()
    {
        await EnsureAllSongsPlaylistAsync();
    }

    public PlaylistSong AddNewSongToPlaylist(Song newSong, Guid playlistToAddId)
    {
        // Always add song (and link) to All Songs first
        var allSongs = _db.Playlists.First(p => p.Id == AllSongsPlaylistId);
        var allSongsLink = _musicService.AddSong(newSong, allSongs);

        // If target playlist is All Songs, we’re done
        if (playlistToAddId == AllSongsPlaylistId)
            return allSongsLink;

        // Otherwise add another PlaylistSong row reusing the same Song
        Playlist playlistToAdd = _db.Playlists.First(p => p.Id == playlistToAddId);
        var playlistLink = new PlaylistSong(allSongsLink.Song, playlistToAdd);

        _db.PlaylistSongs.Add(playlistLink);
        _db.SaveChanges();

        return playlistLink;
    }

    public PlaylistSong AddExistingSongToPlaylist(Guid songId, Guid playlistToAddId)
    {
        var playlist = _db.Playlists.First(p => p.Id == playlistToAddId);
        var song     = _db.Songs.First(s => s.Id == songId);

        var playlistSong = new PlaylistSong(song, playlist);
        _db.PlaylistSongs.Add(playlistSong);
        _db.SaveChanges();

        return playlistSong;
    }


    public IEnumerable<PlaylistSong> GetSongsForPlaylist(Guid playlistId)
    {
        return _musicService.GetSongsForPlaylist(playlistId);
    }

    public IEnumerable<PlaylistSong> GetAllSongsPlaylist()
    {
        var allSongsPlaylist = _db.Playlists
            .Include(playlist => playlist.PlaylistItems)
            .FirstOrDefault(p => p.Id == AllSongsPlaylistId);
        allSongsPlaylist ??= EnsureAllSongsPlaylistAsync().Result;
        return allSongsPlaylist.PlaylistItems;
    }

    public IEnumerable<Playlist> GetAllPlaylists()
    {
        return _musicService.GetAllPlaylists();
    }

    public Playlist CreatePlaylist(string playlistWindowPlaylistName)
    {
        return _musicService.CreatePlaylist(playlistWindowPlaylistName);
    }

    public void DeletePlaylist(Guid playlistId)
    {
        _musicService.DeletePlaylist(playlistId);
    }

    public void MarkLastPlayedPlaylist(Guid playlistId)
    {
        throw new NotImplementedException();
    }

    public async Task<Playlist> EnsureAllSongsPlaylistAsync()
    {
        const string AllSongsName = "All Songs";
        var playlist = await _db.Playlists
            .FirstOrDefaultAsync(p => p.Name == AllSongsName);

        if (playlist == null)
        {
            playlist = new Playlist(AllSongsName, true);
            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();
        }

        AllSongsPlaylistId = playlist.Id;
        return playlist;
    }
}

public interface ILibraryManager
{
    Guid AllSongsPlaylistId { get; }
    void Initialize();
    Task<Playlist> EnsureAllSongsPlaylistAsync();
    PlaylistSong AddNewSongToPlaylist(Song newSong, Guid playlistToAddId);
    PlaylistSong AddExistingSongToPlaylist(Guid songId, Guid playlistToAddId);
    IEnumerable<PlaylistSong> GetSongsForPlaylist(Guid playlistId);
    IEnumerable<PlaylistSong> GetAllSongsPlaylist();
    IEnumerable<Playlist> GetAllPlaylists();
    Playlist CreatePlaylist(string playlistWindowPlaylistName);
    void DeletePlaylist(Guid playlistId);
    void MarkLastPlayedPlaylist(Guid playlistId);
}