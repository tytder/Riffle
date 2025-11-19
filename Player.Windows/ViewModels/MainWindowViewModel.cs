using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    
    public Song? CurrentSong => _playbackManager.CurrentSong;

    public string SelectedPlaylistInfo => GetPlaylistInfo();
    public ObservableQueue<Song> Queue => _playbackManager.Queue;
    public ObservableQueue<SongPlayed> RecentlyPlayed => _playbackManager.RecentlyPlayed;
    public bool IsLooping => _playbackManager.IsLooping;

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
        _playbackManager = new PlaybackManager(player);
        _playbackManager.PropertyChanged += Playback_PropertyChanged;
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
    public void PlayFrom(PlaylistViewModel? selectedPlaylistViewModel, Song? songToPlay = null)
    {
        // Decide the concrete list of songs to play:
        // - if selectedVm is null or represents "All Songs" (Playlist == null) -> use the songs currently shown in the SongsViewModel
        // - otherwise ask the MusicService for the songs that belong to that playlist  
        List<Song> playListSongs = selectedPlaylistViewModel?.Playlist == null ? 
            SongsViewModel.GetAllSongs() :
            _musicService.GetSongsForPlaylist(selectedPlaylistViewModel.Playlist);

        songToPlay ??= selectedPlaylistViewModel?.GetFirstSong() ?? playListSongs[0];

        // Update "currently playing" state in the MainWindowViewModel
        CurrentPlaylistPlaying = selectedPlaylistViewModel;
        
        // Start playback
        _playbackManager.PlayFrom(songToPlay, playListSongs);
    }
    
    private void Playback_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaybackManager.CurrentSong))
        {
            OnPropertyChanged(nameof(CurrentSong));
        }
    }

    public void AddSong(Song newSong, Playlist? playlist)
    {
        _musicService.AddSong(newSong, playlist);
        
        // Refresh the songs in the viewmodel
        SongsViewModel.RefreshSongs();
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
        // TODO: remove playlist should stop the rest of the (playlisted) queue and should reset its stored playlist
        if (selectedVmPlaylist.Playlist != null) _musicService.DeletePlaylist(selectedVmPlaylist.Playlist);
        SidebarViewModel.RemovePlaylist(selectedVmPlaylist);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}