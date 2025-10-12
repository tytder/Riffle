using System.Collections.ObjectModel;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class SongsViewModel
{
    private readonly MusicService _musicService;
    private PlaylistViewModel? _currentPlaylistVm;

    public ObservableCollection<Song> Songs { get; } = new();

    public SongsViewModel(MusicService musicService)
    {
        _musicService = musicService;
    }

    public void LoadSongs(PlaylistViewModel playlistVm)
    {
        _currentPlaylistVm = playlistVm;
        RefreshSongs();
    }

    public void RefreshSongs()
    {
        if (_currentPlaylistVm == null)
            return;

        Songs.Clear();

        var songs = _currentPlaylistVm.Playlist == null
            ? _musicService.GetAllSongs() // “All Songs”
            : _musicService.GetSongsForPlaylist(_currentPlaylistVm.Playlist);

        foreach (var song in songs)
        {
            Songs.Add(song);
        }
    }
}