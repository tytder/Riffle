using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly MusicService _musicService;

    public SidebarViewModel SidebarViewModel { get; }
    public SongsViewModel SongsViewModel { get; }

    private PlaylistViewModel? _selectedPlaylist;
    public PlaylistViewModel? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (!Equals(_selectedPlaylist, value))
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
                SongsViewModel.LoadSongs(value!);
            }
        }
    }

    private PlaylistViewModel? _currentPlaylistPlaying;
    public PlaylistViewModel? CurrentPlaylistPlaying
    {
        get => _currentPlaylistPlaying;
        set
        {
            if (!Equals(_currentPlaylistPlaying, value))
            {
                _currentPlaylistPlaying = value;
                OnPropertyChanged();
            }
        }
    }

    private Song? _currentSongPlaying;
    public Song? CurrentSongPlaying
    {
        get => _currentSongPlaying;
        set
        {
            if (!Equals(_currentSongPlaying, value))
            {
                _currentSongPlaying = value;
                OnPropertyChanged();
            }
        }
    }

    public MainWindowViewModel(MusicService musicService)
    {
        _musicService = musicService;
        SidebarViewModel = new SidebarViewModel(musicService);
        SongsViewModel = new SongsViewModel(musicService);
        SelectedPlaylist = SidebarViewModel.Playlists[0];
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}