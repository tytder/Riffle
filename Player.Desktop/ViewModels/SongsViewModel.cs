using System.Collections.ObjectModel;
using Riffle.Core.Models;
using Riffle.Data;

namespace Player.Desktop.ViewModels;
#nullable enable
public class SongsViewModel
{
    private readonly ILibraryManager _libraryManager;
    private PlaylistViewModel? _currentPlaylistVm;

    public ObservableCollection<PlaylistSong> PlaylistSongs { get; } = new();

    public SongsViewModel(
        ILibraryManager libraryManager
        )
    {
        if (!libraryManager.IsInitialized) libraryManager.Initialize();
        _libraryManager = libraryManager;
    }

    public void LoadSongs(PlaylistViewModel? playlistVm)
    {
        _currentPlaylistVm = playlistVm;
        RefreshSongs();
    }

    public void RefreshSongs()
    {
        if (_currentPlaylistVm == null)
            return;

        PlaylistSongs.Clear();

        var songs = _libraryManager.GetSongsForPlaylist(_currentPlaylistVm.Playlist.Id);

        foreach (var song in songs)
        {
            PlaylistSongs.Add(song);
        }
    }
}