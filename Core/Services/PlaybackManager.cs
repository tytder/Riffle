#nullable enable
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Utilities;

namespace Riffle.Core.Services;

public class PlaybackManager : INotifyPropertyChanged
{
    public ObservableQueue<PlaylistSong> Queue; // TODO: currently this is the TotalQueue, change to be user queued songs and then play those first before playing source songs
    public event NotifyCollectionChangedEventHandler? QueueCollectionChanged;
    public ObservableQueue<SongPlayed> RecentlyPlayed;
    
    private readonly IAudioPlayer _player;
    private Playlist? _playingPlaylist;
    private List<PlaylistSong>? _playlistSource;

    public ObservableQueue<PlaylistSong> TotalQueue;
    
    private SongPlayed? _currentSong;
    public SongPlayed? CurrentSong
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
    
    public PlaybackManager(IAudioPlayer audioPlayer)
    {
        _player = audioPlayer;
        _player.TrackEnded += PlayerOnTrackEnded;
        RecentlyPlayed = new ObservableQueue<SongPlayed>(50, true);
        Queue = new ObservableQueue<PlaylistSong>();
        Queue.CollectionChanged += OnQueueCollectionChanged;
        TotalQueue = new ObservableQueue<PlaylistSong>();
    }

    private void OnQueueCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var handler = QueueCollectionChanged;
        handler?.Invoke(sender, e);
    }

    public void PlayFrom(PlaylistSong song, Playlist? playlist)
    {
        Stop();
        if (playlist == null) return;
        var songs = playlist.PlaylistItems.ToList();
        if (!songs.Contains(song)) return;

        _playingPlaylist = playlist;
        _playlistSource = songs;

        RecreateTotalQueue(song);
        
        var curSong = TotalQueue.Peek();
        CurrentSong = new SongPlayed(curSong.Song, DateTime.UtcNow, playlist);
        _player.Play(CurrentSong);
    }

    public void Stop()
    {
        if (CurrentSong != null)
        {
            var handler = TrackStopped;
            handler?.Invoke(this, new TrackEventArgs(CurrentSong));
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
        //int index = _playlistSource.IndexOf(CurrentSong) + 1;
        int index = _playlistSource.FindIndex(ps => ps.Song.Equals(CurrentSong.Song)) + 1;
        
        if (index >= _playlistSource.Count)
        {
            if (IsLooping)
                index = 0;
            else
            {
                if (naturallyEnded)
                {
                    Stop();
                    _playlistSource?.Clear();
                    return;
                }
                else
                {
                    Stop();
                    return;
                    // TODO: should stop playing, but not clear queue, and then when user skips again it or presses play should start playing the first song again
                    //index = 0;
                }
            }
        }

        PlayFrom(_playlistSource[index], _playingPlaylist);
    }

    public void SkipToPrevSong()
    {
        if (_playlistSource == null) return;
        if (_playlistSource.Count == 0 || CurrentSong == null)
            return;

        //int index = _playlistSource.IndexOf(CurrentSong) - 1;
        int index = _playlistSource.FindIndex(ps => ps.Song.Equals(CurrentSong.Song)) - 1;
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
        //var startIndex = _playlistSource.IndexOf(CurrentSong);
        int startIndex = _playlistSource.FindIndex(ps => ps.Song.Equals(CurrentSong.Song));
        for (var index = 0; index < _playlistSource.Count; index++)
        {
            if (IsLooping)
            {
                var playlistSong = _playlistSource[(startIndex + index) % _playlistSource.Count];
                if (TotalQueue.Contains(playlistSong)) continue;
                TotalQueue.Enqueue(playlistSong);
            }
            else
            {
                if (index >= startIndex) break;
                var playlistSong = _playlistSource[index];
                if (!TotalQueue.Contains(playlistSong)) continue;
                TotalQueue.Remove(playlistSong);
            }
        }
    }

    private void RecreateTotalQueue(PlaylistSong song)
    {
        if (_playlistSource == null)
            throw new NullReferenceException(
                $"{nameof(RecreateTotalQueue)} was called while {nameof(_playlistSource)} is null)");
        var startIndex = 0;
        if (!Queue.Contains(song))
        {
            startIndex = _playlistSource.IndexOf(song);
        }
        var ordered = _playlistSource.Skip(startIndex).Concat(_playlistSource.Take(IsLooping ? startIndex : 0));
        var queue = Queue.ToList();
        queue.AddRange(ordered);
        TotalQueue = new ObservableQueue<PlaylistSong>(queue);
    }
    
    // TODO: Look into why natural end of last song of ghost playlist doesnt switch to no selected song
    public void OnPlaylistRemoved(object? sender, PlaylistEventArgs e)
    {
        if (!e.Playlist.Equals(_playingPlaylist)) return;
        
        _playingPlaylist = null;
    }
    
    public void ClearUserQueue()
    {
        Queue.Clear();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}