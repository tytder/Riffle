using System.Collections.ObjectModel;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class SongsViewModel
{
    private readonly MusicService _musicService;

    public ObservableCollection<Song> Songs { get; } = new();

    public SongsViewModel(MusicService musicService)
    {
        _musicService = musicService;
    }

    public void LoadSongs(PlaylistViewModel playlistVm)
    {
        Songs.Clear();

        var songs = playlistVm.Playlist == null
            ? _musicService.GetAllSongs() // All Songs
            : _musicService.GetSongsForPlaylist(playlistVm.Playlist);

        foreach (var song in songs)
        {
            Songs.Add(song);
        }
    }
}