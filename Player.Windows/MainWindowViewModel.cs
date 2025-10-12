using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly MusicService _musicService;

    public SidebarViewModel Sidebar { get; }
    public SongsViewModel Songs { get; }

    private PlaylistViewModel? _selectedPlaylist;
    public PlaylistViewModel? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (_selectedPlaylist != value)
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
                Songs.LoadSongs(value!);
            }
        }
    }

    private Song? _currentSongPlaying;
    public Song? CurrentSongPlaying
    {
        get => _currentSongPlaying;
        set
        {
            if (_currentSongPlaying != value)
            {
                _currentSongPlaying = value;
                OnPropertyChanged();
            }
        }
    }

    public MainWindowViewModel(MusicService musicService)
    {
        _musicService = musicService;
        Sidebar = new SidebarViewModel(musicService);
        Songs = new SongsViewModel(musicService);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}