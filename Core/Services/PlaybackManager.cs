#nullable enable
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
    private readonly IAudioPlayer _player;
    // Queue populated by songs the user manually adds.
    public ObservableCollection<PlaylistSong> UserQueue => _userQueue.PlaylistItems;
    private QueuePlaylist _userQueue; 
    public ObservableQueue<SongPlayed> RecentlyPlayed;
    
    // The total (shuffled) queue, always showing current playing song at the top.
    public ObservableQueue<PlaylistSong> TotalQueue;
    // The total (shuffled) queue as it was when it got created. Not keeping track of where we are in the playlist.
    private List<PlaylistSong> _nonTrackingTotalQueue;
    // List of all sources of which the non tracking total queue consists of.
    private List<Playlist> _sources;
    
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
    
    // int representing the int of the index of the currentsong in the non tracking total queue.
    private int _currentIndex;
    
    public bool IsLooping { get; private set; }
    public event EventHandler<TrackEventArgs>? TrackStopped;
    public event NotifyCollectionChangedEventHandler? UserQueueCollectionChanged;
    
    public PlaybackManager(IAudioPlayer audioPlayer)
    {
        _player = audioPlayer;
        _player.TrackEnded += PlayerOnTrackEnded;
        RecentlyPlayed = new ObservableQueue<SongPlayed>(50, true);
        _userQueue = new QueuePlaylist("Queue");
        _userQueue.QueuePlaylistItems.CollectionChanged += OnUserQueueCollectionChanged;
        _nonTrackingTotalQueue = new List<PlaylistSong>();
        TotalQueue = new ObservableQueue<PlaylistSong>();
        _sources = [_userQueue];
    }

    private void OnUserQueueCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(UserQueue));
        var handler = UserQueueCollectionChanged;
        handler?.Invoke(sender, e);
    }

    public void PlayFrom(PlaylistSong song)
    {
        Stop();

        // if user started playing from new playlist, clear current sources and rebuild everything.
        if (!_sources.Contains(song.Playlist))
        {
            RecreateSources(song);
        }
        
        RecreateTotalQueue(song);
        CurrentSong = new SongPlayed(song, DateTime.UtcNow);
        _player.Play(CurrentSong!);
    }

    public void Stop()
    {
        if (CurrentSong != null)
        {
            var handler = TrackStopped;
            handler?.Invoke(this, new TrackEventArgs(CurrentSong!));
            RecentlyPlayed.Enqueue(CurrentSong);
            OnPropertyChanged(nameof(RecentlyPlayed));
        }
        _player.StopAll();
    }

    private void PlayerOnTrackEnded(object? sender, EventArgs e)
    {
        SkipToNextSong(true);
    }

    public void SkipToNextSong(bool naturallyEnded = false)
    {
        if (_nonTrackingTotalQueue.Count == 0 || CurrentSong == null)
            return;
        
        _currentIndex++;
        
        if (_currentIndex >= _nonTrackingTotalQueue.Count)
        {
            if (IsLooping)
                _currentIndex = 0;
            else
            {
                if (naturallyEnded)
                {
                    Stop();
                    _nonTrackingTotalQueue?.Clear();
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

        PlayFrom(_nonTrackingTotalQueue[_currentIndex]);
    }

    public void SkipToPrevSong()
    {
        if (_nonTrackingTotalQueue.Count == 0 || CurrentSong == null)
            return;

        if (_currentIndex >= 0) _currentIndex--;
        
        if (_currentIndex < 0)
        {
            if (IsLooping)
                _currentIndex = _nonTrackingTotalQueue.Count - 1;
            else
            {
                Stop();
                return;
            }
        }

        PlayFrom(_nonTrackingTotalQueue[_currentIndex]);
    }

    public void ToggleLoop()
    {
        IsLooping = !IsLooping;
        if (CurrentSong == null) return;
        RecreateTotalQueue();
        //var startIndex = _playlistSource.IndexOf(CurrentSong);
        /*int startIndex = _nonTrackingTotalQueue.FindIndex(ps => ps.Song.Equals(CurrentSong.Song));
        for (var index = 0; index < _nonTrackingTotalQueue.Count; index++)
        {
            if (IsLooping)
            {
                var playlistSong = _nonTrackingTotalQueue[(startIndex + index) % _nonTrackingTotalQueue.Count];
                if (TotalQueue.Contains(playlistSong)) continue;
                TotalQueue.Enqueue(playlistSong);
            }
            else
            {
                if (index >= startIndex) break;
                var playlistSong = _nonTrackingTotalQueue[index];
                if (!TotalQueue.Contains(playlistSong)) continue;
                TotalQueue.Remove(playlistSong);
            }
        }*/
    }

    private void RecreateSources(PlaylistSong song, bool reshuffle = true)
    {
        if (!_sources.Contains(song.Playlist)) _sources.Add(song.Playlist);
        RecreateSources(reshuffle);
    }
    
    private void RecreateSources(bool reshuffle = true)
    {
        if (reshuffle)
        {
            _nonTrackingTotalQueue = _sources.SelectMany(s => s.PlaylistItems).ToList();
        }
        // TODO: shuffle logic
        /*if (reshuffle)
        {
            
        }*/
    }

    private void RecreateTotalQueue(PlaylistSong? song = null)
    {
        if (_nonTrackingTotalQueue == null)
            throw new NullReferenceException(
                $"{nameof(RecreateTotalQueue)} was called while {nameof(_nonTrackingTotalQueue)} is null)");
        var startIndex = _currentIndex;
        if (song != null)
        {
            if (_nonTrackingTotalQueue.Contains(song))
            {
                startIndex = _nonTrackingTotalQueue.IndexOf(song);
            }
        }
        var ordered = _nonTrackingTotalQueue.Skip(startIndex).Concat(_nonTrackingTotalQueue.Take(IsLooping ? startIndex : 0));
        //TotalQueue = new ObservableQueue<PlaylistSong>(ordered);
        TotalQueue.Clear();
        TotalQueue.AddRange(ordered);
        OnPropertyChanged(nameof(TotalQueue));
        _currentIndex = startIndex;
    }
    
    // TODO: Look into why natural end of last song of ghost playlist doesnt switch to no selected song
     public void OnPlaylistRemoved(object? sender, PlaylistEventArgs e)
    {
        if (!_sources.Contains(e.Playlist)) return;

        _sources.Remove(e.Playlist);
        foreach (var playlistItem in e.Playlist.PlaylistItems.ToArray())
        {
            // Only remove the songs from the deleted playlist if they aren't in the user's queue.
            //if (_nonTrackingTotalQueue.Contains(playlistItem) && !_userQueue.PlaylistItems.Contains(playlistItem)) 
                _nonTrackingTotalQueue.Remove(playlistItem);
        }
        /*
            if (!e.Playlist.Equals(_playingPlaylist)) return;

            _playingPlaylist = null;
        */
    }

    public void AddToUserQueue(PlaylistSong songToAdd)
    {
        var queueSong = new PlaylistSong(songToAdd, _userQueue);
        _userQueue.QueuePlaylistItems.Add(queueSong);
        RecreateSources();
        RecreateTotalQueue();
    }
    
    public void ClearUserQueue()
    {
        _userQueue.QueuePlaylistItems.Clear();
        RecreateSources();
        RecreateTotalQueue();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}