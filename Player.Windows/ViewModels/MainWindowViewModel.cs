using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Models;
using Riffle.Core.Services;
using Riffle.Core.Utilities;
using Riffle.Player.Windows.Services;

namespace Riffle.Player.Windows.ViewModels;
#nullable enable
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly MusicService _musicService;
    private readonly PlaybackManager _playbackManager;

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

    public string? CurrentPlaylistName => CurrentPlaylistPlaying?.Name; 
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
                OnPropertyChanged(nameof(CurrentPlaylistName));
                OnPropertyChanged(nameof(IsCurrentPlayingPlaylistQueueVisible));
            }
        }
    }
    public bool IsCurrentPlayingPlaylistQueueVisible => CurrentPlaylistPlaying != null;

    public string CurrentSongTitle => _playbackManager.CurrentSong?.Title ?? "No song selected";
    public string CurrentSongArtist => _playbackManager.CurrentSong?.Artist ?? "";
    public Song? CurrentSong => _playbackManager.CurrentSong;

    public string SelectedPlaylistInfo => GetPlaylistInfo();
    public ObservableQueue<Song> TotalQueue => _playbackManager.TotalQueue;
    public ObservableQueue<Song> Queue => _playbackManager.Queue;
    public bool IsQueueVisible => Queue.Count > 0;
    
    private bool _isQueueWindowOpen;
    public bool IsQueueWindowOpen
    {
        get => _isQueueWindowOpen;
        set
        {
            if (!Equals(_isQueueWindowOpen, value))
            {
                _isQueueWindowOpen = value;
                OnPropertyChanged();
            }
        }
    }
    public ObservableQueue<SongPlayed> RecentlyPlayed => _playbackManager.RecentlyPlayed;
    public bool IsLooping => _playbackManager.IsLooping;
    public event EventHandler<PlaylistEventArgs>? PlaylistRemoved;

    private string GetPlaylistInfo()
    {
        var playlist = SelectedPlaylist?.Playlist?.PlaylistItems.ToList() ?? _musicService.GetAllSongs();
        var count = playlist.Count;
        var totalDuration = TimeSpan.FromSeconds(playlist.Sum(s => s.Duration.TotalSeconds));
        return $"{count} songs, {(int)totalDuration.TotalHours} hr {totalDuration.Minutes} min";
    }

    public MainWindowViewModel(MusicService musicService, NAudioAudioPlayer player)
    {
        _musicService = musicService;
        SidebarViewModel = new SidebarViewModel(musicService);
        SongsViewModel = new SongsViewModel(musicService);
        SelectedPlaylist = SidebarViewModel.Playlists[0];
        _playbackManager = new PlaybackManager(player, _musicService.GetAllSongs);
        _playbackManager.PropertyChanged += Playback_PropertyChanged;
        PlaylistRemoved += _playbackManager.OnPlaylistRemoved;
    }

    public void ToggleLoop()
    {
        _playbackManager.ToggleLoop();
    }

    public void SkipToNextSong()
    {
        _playbackManager.SkipToNextSong();
    }

    public void SkipToPrevSong()
    {
        _playbackManager.SkipToPrevSong();
    }

    /// <summary>
    /// Starts playing any song.
    /// </summary>
    /// <param name="selectedPlaylistViewModel">The current selected playlist.</param>
    /// <param name="songToPlay">The song to start playing. Leave selectedSong null to play first song of current open playlist.</param>
    public void PlayFrom(PlaylistViewModel selectedPlaylistViewModel, Song? songToPlay = null)
    {
        // Decide the concrete list of songs to play:
        // - just grab the current playlist
        // - or if selectedVm is null -> represents "All Songs"
        // GetAllSongs already handles if there is no AllSongs playlist so we can ignore the null warning
        Playlist? playlist = selectedPlaylistViewModel.Playlist;
        songToPlay ??= GetFirstSong(selectedPlaylistViewModel.Playlist);

        // Update "currently playing" state in the MainWindowViewModel
        CurrentPlaylistPlaying = selectedPlaylistViewModel;
        
        // Start playback
        _playbackManager.PlayFrom(songToPlay, playlist);
    }

    private Song GetFirstSong(Playlist? selectPlaylist)
    {
        var playlist = selectPlaylist?.PlaylistItems.ToList() ?? _musicService.GetAllSongs();
        return playlist[0]; // TODO: take into account shuffle logic
    }

    private void Playback_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaybackManager.CurrentSong))
        {
            OnPropertyChanged(nameof(CurrentSong));
            OnPropertyChanged(nameof(CurrentSongTitle));
            OnPropertyChanged(nameof(CurrentSongArtist));
        }
    }

    public void AddSong(Song newSong, Playlist? playlist)
    {
        _musicService.AddSong(newSong, playlist);
        
        playlist?.PlaylistItems.Add(newSong);
        
        // Refresh the songs in the viewmodel
        SongsViewModel.LoadSongs(SelectedPlaylist);
    }

    public PlaylistViewModel CreatePlaylist(string playlistWindowPlaylistName)
    {
        return SidebarViewModel.AddPlaylist(_musicService.CreatePlaylist(playlistWindowPlaylistName));
    }

    public PlaylistViewModel? GetPlaylist(Guid newPlaylistId)
    {
        return SidebarViewModel.GetPlaylist(newPlaylistId);
    }

    public PlaylistViewModel GetAllSongsPlaylist()
    {
        return SidebarViewModel.GetAllSongsPlaylist();
    }
    
    public void DeletePlaylist(PlaylistViewModel selectedVmPlaylist)
    {
        if (selectedVmPlaylist.Playlist != null) _musicService.DeletePlaylist(selectedVmPlaylist.Playlist);
        SidebarViewModel.RemovePlaylist(selectedVmPlaylist);
        var handler = PlaylistRemoved;
        handler?.Invoke(this, new PlaylistEventArgs(selectedVmPlaylist.Playlist));
    }
    
    public void ClearUserQueue()
    {
        _playbackManager.ClearUserQueue();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}