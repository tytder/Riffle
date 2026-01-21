using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Core.Models;
using Riffle.Data;
using Riffle.Player.Windows.Services;

namespace Riffle.Player.Windows.ViewModels;
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

        var songs = _currentPlaylistVm.Playlist == null
            ? _libraryManager.GetAllSongsPlaylist() // “All Songs”
            : _libraryManager.GetSongsForPlaylist(_currentPlaylistVm.Playlist);

        foreach (var song in songs)
        {
            PlaylistSongs.Add(song);
        }
    }
}