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

    public PlaylistSong AddSong(Song newSong, Playlist playlistToAdd)
    {
        // Always add song (and link) to All Songs first
        var allSongs = _db.Playlists.First(p => p.Id == AllSongsPlaylistId);
        var allSongsLink = _musicService.AddSong(newSong, allSongs);

        // If target playlist is All Songs, we’re done
        if (playlistToAdd.Id == AllSongsPlaylistId)
            return allSongsLink;

        // Otherwise add another PlaylistSong row reusing the same Song
        var playlistLink = new PlaylistSong(allSongsLink.Song, playlistToAdd);

        _db.PlaylistSongs.Add(playlistLink);
        _db.SaveChanges();

        return playlistLink;
    }


    public IEnumerable<PlaylistSong> GetSongsForPlaylist(Playlist playlist)
    {
        return _musicService.GetSongsForPlaylist(playlist);
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

    public void DeletePlaylist(Playlist playlist)
    {
        _musicService.DeletePlaylist(playlist);
    }

    private async Task<Playlist> EnsureAllSongsPlaylistAsync()
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
    PlaylistSong AddSong(Song newSong, Playlist playlistToAdd);
    IEnumerable<PlaylistSong> GetSongsForPlaylist(Playlist playlist);
    IEnumerable<PlaylistSong> GetAllSongsPlaylist();
    IEnumerable<Playlist> GetAllPlaylists();
    Playlist CreatePlaylist(string playlistWindowPlaylistName);
    void DeletePlaylist(Playlist playlist);
}