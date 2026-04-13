using Riffle.Core.Models;

namespace Riffle.Data;

public interface ILibraryManager
{
    Guid AllSongsPlaylistId { get; }
    bool IsInitialized { get; }
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