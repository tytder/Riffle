using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Player.Desktop.Models;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Services;
using Riffle.Core.Utilities;
using Riffle.Data;
using RelayCommand = Player.Commands.RelayCommand;

namespace Player.Desktop.ViewModels;
#nullable enable
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly PlaybackManager _playbackManager;
    protected PlaybackManager PlaybackManager => _playbackManager;
    private readonly ILibraryManager _libraryManager;
    private readonly IAudioPlayer _player;
    public IAudioPlayer Player => _player;

    public SidebarViewModel SidebarViewModel { get; }
    public SongsViewModel SongsViewModel { get; }

    // SelectedPlaylist sets and gets this variable, and SelectedPlaylist is set on constructor so in extension this is as well.
    private PlaylistViewModel _selectedPlaylist = null!; 
    public PlaylistViewModel SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (!Equals(_selectedPlaylist, value))
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPlaylistInfo));
                SongsViewModel.LoadSongs(value);
            }
        }
    }

    public string CurrentSongPlaylistName
    {
        get
        {
            if (CurrentSong == null) return "";
            return CurrentSong.PlayedFromName ?? "Deleted playlist";
        }
    }
    
    // TODO: change to also showcase mixes of playlists later on
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

    public string CurrentSongTitle => _playbackManager.CurrentSong?.SongName ?? "No song selected";
    public string CurrentSongArtist => _playbackManager.CurrentSong?.ArtistName ?? "";
    public SongPlayed? CurrentSong => _playbackManager.CurrentSong;

    public string SelectedPlaylistInfo => GetPlaylistInfo();
    public ObservableQueue<PlaylistSong> TotalQueue => _playbackManager.TotalQueue;
    public ObservableCollection<PlaylistSong> Queue => _playbackManager.UserQueue;
    public bool IsQueueVisible => Queue.Count > 0; // TODO: figure out how to update this.
    
    [ObservableProperty]
    private bool _isQueueWindowOpen;

    public ObservableQueue<SongPlayed> RecentlyPlayed => _playbackManager.RecentlyPlayed;
    public bool IsLooping => _playbackManager.IsLooping;

    [ObservableProperty]
    private bool _isPlaylistsPanelExpanded = true;


    public event EventHandler<PlaylistEventArgs>? PlaylistRemoved;

    private string GetPlaylistInfo()
    {
        var playlist = SelectedPlaylist.Playlist.PlaylistItems.ToArray();
        var count = playlist.Count();
        var totalDuration = TimeSpan.FromSeconds(playlist.Sum(ps => ps.Song.Duration.TotalSeconds));
        return $"{count} songs, {(int)totalDuration.TotalHours} hr {totalDuration.Minutes} min";
    }
    
    public MainWindowViewModel(
        ILibraryManager libraryManager,
        IAudioPlayer player,
        SidebarViewModel sidebarVm,
        SongsViewModel songsVm,
        PlaybackManager playbackManager)
    {
        _libraryManager = libraryManager;
        SidebarViewModel = sidebarVm;
        //SidebarViewModel = new SidebarViewModel(musicService);
        SongsViewModel = songsVm;
        //SongsViewModel = new SongsViewModel(musicService);
        SelectedPlaylist = SidebarViewModel.Playlists[0];

        _player = player;
        
        _playbackManager = playbackManager;
        _playbackManager.PropertyChanged += PlaybackPropertyChanged;
        _playbackManager.UserQueueCollectionChanged += PlaybackUserQueueChanged;
        PlaylistRemoved += _playbackManager.OnPlaylistRemoved;
    }

    ~MainWindowViewModel()
    {
        if (_playbackManager != null)
        {
            _playbackManager.PropertyChanged -= PlaybackPropertyChanged;
            _playbackManager.UserQueueCollectionChanged -= PlaybackUserQueueChanged;
            PlaylistRemoved -= _playbackManager.OnPlaylistRemoved;
        }
    }

    private void PlaybackUserQueueChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Queue));
        OnPropertyChanged(nameof(IsQueueVisible));
    }

    [RelayCommand]
    private void AddToQueue(object? selectedItem)
    {
        if (selectedItem is not PlaylistSong playlistSong) return;
        
        _playbackManager.AddToUserQueue(playlistSong);
    }
    
    [RelayCommand]
    private void TogglePlaylistsPanel() => IsPlaylistsPanelExpanded = !IsPlaylistsPanelExpanded;
    
    [RelayCommand]
    private void ToggleQueuePanel() => IsQueueWindowOpen = !IsQueueWindowOpen;
    

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
    public void PlayFrom(PlaylistViewModel selectedPlaylistViewModel, PlaylistSong? songToPlay = null)
    {
        // Decide the concrete list of songs to play:
        // - just grab the current playlist
        // - or if selectedVm is null -> represents "All Songs"
        // GetAllSongs already handles if there is no AllSongs playlist so we can ignore the null warning
        Playlist playlist = selectedPlaylistViewModel.Playlist;
        songToPlay ??= GetFirstSong(selectedPlaylistViewModel.Playlist);
        if (songToPlay == null) return;

        // Update "currently playing" state in the MainWindowViewModel
        CurrentPlaylistPlaying = selectedPlaylistViewModel;
        
        // Start playback
        _playbackManager.PlayFrom(songToPlay);
    }

    private PlaylistSong? GetFirstSong(Playlist selectPlaylist)
    {
        var playlist
            = selectPlaylist.PlaylistItems.ToArray();
        if (playlist.Length <= 0) return null;
        return playlist[0]; // TODO: take into account shuffle logic
    }

    private void PlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_playbackManager.CurrentSong):
                OnPropertyChanged(nameof(CurrentSong));
                OnPropertyChanged(nameof(CurrentSongTitle));
                OnPropertyChanged(nameof(CurrentSongArtist));
                OnPropertyChanged(nameof(CurrentSongPlaylistName));
                break;
            case nameof(_playbackManager.RecentlyPlayed):
                OnPropertyChanged(nameof(RecentlyPlayed));
                break;
            case nameof(_playbackManager.TotalQueue):
                OnPropertyChanged(nameof(TotalQueue));
                break;
        }
    }

    public PlaylistSong AddSong(Song newSong, Playlist playlist)
    {
        var newPlaylistSong = _libraryManager.AddNewSongToPlaylist(newSong, playlist.Id);
        
        // Refresh the songs in the viewmodel
        SongsViewModel.LoadSongs(SelectedPlaylist);

        return newPlaylistSong;
    }

    public PlaylistViewModel CreatePlaylist(string playlistWindowPlaylistName)
    {
        return SidebarViewModel.AddPlaylist(_libraryManager.CreatePlaylist(playlistWindowPlaylistName));
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
        _libraryManager.DeletePlaylist(selectedVmPlaylist.Playlist.Id);
        SidebarViewModel.RemovePlaylist(selectedVmPlaylist);
        var handler = PlaylistRemoved;
        handler?.Invoke(this, new PlaylistEventArgs(selectedVmPlaylist.Playlist));
    }
    
    public void ClearUserQueue()
    {
        _playbackManager.ClearUserQueue();
    }
    
    public PlaylistViewModel? GetPlaylistModel(SongPlayed viewModelCurrentSong)
    {
        PlaylistViewModel? playlistViewModel = null;
        if (viewModelCurrentSong.PlaylistId.HasValue)
            playlistViewModel = SidebarViewModel.GetPlaylist(viewModelCurrentSong.PlaylistId.Value);
        if (viewModelCurrentSong.Playlist != null)
            playlistViewModel ??= SidebarViewModel.AddPlaylist(viewModelCurrentSong.Playlist);
        return playlistViewModel;
    }

    public void OpenPlaylist(PlaylistViewModel? selectedVm = null)
    {
        selectedVm ??= CurrentPlaylistPlaying;
        selectedVm ??= SelectedPlaylist;
        SongsViewModel.LoadSongs(selectedVm);
        SetPlaylistHeaderPlaying(_player.IsPlaying, selectedVm);
        SelectedPlaylist = selectedVm;
    }
    
    public void SetPlaylistHeaderPlaying(bool isPlaying, PlaylistViewModel? playlistViewModel = null)
    {
        playlistViewModel ??= SelectedPlaylist;

        if (isPlaying && Equals(CurrentPlaylistPlaying, playlistViewModel))
        {
            /*PlaylistPlayBtn.Content = "⏸";
            PlaylistPlayBtn.Padding = new Thickness(-2, -2, -2, -0);
            PlaylistPlayBtn.FontSize = 18;*/
        }
        else
        {
            /*PlaylistPlayBtn.Content = "▶";
            PlaylistPlayBtn.Padding = new Thickness(5, -1, 5, 0);
            PlaylistPlayBtn.FontSize = 12;*/
        }
    }
}