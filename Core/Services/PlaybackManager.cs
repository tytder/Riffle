#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Utilities;

namespace Riffle.Core.Services;

public class PlaybackManager : INotifyPropertyChanged
{
    public ObservableQueue<Song> Queue = new();
    public ObservableQueue<SongPlayed> RecentlyPlayed;
    
    private readonly IAudioPlayer _player;
    private readonly Func<List<Song>> _getAllSongsMethod;
    private Playlist? _playingPlaylist;
    private List<Song>? _playlistSource;
    
    private Song? _currentSong;

    public Song? CurrentSong
    {
        get => _currentSong;
        set
        {
            if (!Equals(_currentSong, value))
            {
                _currentSong = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsLooping { get; private set; }
    public event EventHandler<TrackEventArgs>? TrackStopped;
    
    public PlaybackManager(IAudioPlayer audioPlayer, Func<List<Song>> getAllSongsMethod)
    {
        _player = audioPlayer;
        _getAllSongsMethod = getAllSongsMethod;
        _player.TrackEnded += PlayerOnTrackEnded;
        RecentlyPlayed = new ObservableQueue<SongPlayed>(50, true);
    }

    public void PlayFrom(Song song, Playlist? playlist)
    {
        Stop();

        var songs = playlist?.PlaylistItems.ToList() ?? _getAllSongsMethod.Invoke();
        if (!songs.Contains(song)) return;

        _playingPlaylist = playlist;
        _playlistSource = songs;

        RecreateQueue(song);
        
        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);
    }

    public void Stop()
    {
        if (CurrentSong != null)
        {
            var handler = TrackStopped;
            handler?.Invoke(this, new TrackEventArgs(CurrentSong));
            var previousSong = new SongPlayed(CurrentSong, DateTime.Now);
            RecentlyPlayed.Enqueue(previousSong);
        }
        _player.StopAll();
    }

    private void PlayerOnTrackEnded(object? sender, EventArgs e)
    {
        SkipToNextSong(true);
    }

    public void SkipToNextSong(bool naturallyEnded = false)
    {
        if (_playlistSource == null) return;
        if (_playlistSource.Count == 0 || CurrentSong == null)
            return;
        int index = _playlistSource.IndexOf(CurrentSong) + 1;


        if (_playingPlaylist == null)
        {
            if (naturallyEnded)
            {
                Stop();
                _playlistSource?.Clear();
                return;
            }
            else
            {
                // TODO: should stop playing, but not clear queue, and then when user skips again it or presses play should start playing the first song again
                //index = 0;
            }
        }
        
        if (index >= _playlistSource.Count)
        {
            if (IsLooping)
                index = 0;
            else
            {
                Stop();
                return;
            }
        }

        PlayFrom(_playlistSource[index], _playingPlaylist);
    }

    public void SkipToPrevSong()
    {
        if (_playlistSource == null) return;
        if (_playlistSource.Count == 0 || CurrentSong == null)
            return;

        int index = _playlistSource.IndexOf(CurrentSong) - 1;
        if (index < 0)
        {
            if (IsLooping)
                index = _playlistSource.Count - 1;
            else
            {
                Stop();
                return;
            }
        }

        PlayFrom(_playlistSource[index], _playingPlaylist);
    }

    public void ToggleLoop()
    {
        IsLooping = !IsLooping;
        if (CurrentSong == null) return;
        if (_playlistSource == null) return;
        var startIndex = _playlistSource.IndexOf(CurrentSong);
        for (var index = 0; index < _playlistSource.Count; index++)
        {
            if (IsLooping)
            {
                var playlistSong = _playlistSource[(startIndex + index) % _playlistSource.Count];
                if (Queue.Contains(playlistSong)) continue;
                Queue.Enqueue(playlistSong);
            }
            else
            {
                if (index >= startIndex) break;
                var playlistSong = _playlistSource[index];
                if (!Queue.Contains(playlistSong)) continue;
                Queue.Remove(playlistSong);
            }
        }
    }

    private void RecreateQueue(Song song)
    {
        if (_playlistSource == null)
            throw new NullReferenceException(
                $"{nameof(RecreateQueue)} was called while {nameof(_playlistSource)} is null)");
        var startIndex = _playlistSource.IndexOf(song);
        var ordered = _playlistSource.Skip(startIndex).Concat(_playlistSource.Take(IsLooping ? startIndex : 0));
        Queue = new ObservableQueue<Song>(ordered);
    }
    
    // TODO: Look into why natural end of last song of ghost playlist doesnt switch to no selected song
    public void OnPlaylistRemoved(object? sender, PlaylistEventArgs e)
    {
        if (!e.Playlist?.Equals(_playingPlaylist) ?? false) return;
        
        _playingPlaylist = null;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}