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
    private List<Song> _playlistSource = new();
    
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
    
    public PlaybackManager(IAudioPlayer audioPlayer)
    {
        _player = audioPlayer;
        _player.TrackEnded += PlayerOnTrackEnded;
        RecentlyPlayed = new ObservableQueue<SongPlayed>(50, true);
    }

    public void PlayFrom(Song? song, List<Song> playlist)
    {
        if (CurrentSong != null)
        {
            TrackStopped?.Invoke(this, new TrackEventArgs(CurrentSong));
            var previousSong = new SongPlayed(CurrentSong, DateTime.Now);
            RecentlyPlayed.Enqueue(previousSong);
        }
        
        if (song == null || !playlist.Contains(song)) return;
        
        _playlistSource = playlist.ToList();

        RecreateQueue(song);
        
        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);
    }
    
    private void PlayerOnTrackEnded(object? sender, EventArgs e)
    {
        SkipToNextSong();
    }

    public void SkipToNextSong()
    {
        /*var prevSong = Queue.Dequeue();
        if (IsLooping)
        {
            Queue.Enqueue(prevSong);
        }
        
        if (Queue.Count == 0)
        {
            _player.StopAll();
            return;
        }
        
        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);*/
        
        if (_playlistSource.Count == 0 || CurrentSong == null)
            return;

        int index = _playlistSource.IndexOf(CurrentSong) + 1;

        if (index >= _playlistSource.Count)
        {
            if (IsLooping)
                index = 0;
            else
            {
                _player.StopAll();
                return;
            }
        }

        PlayFrom(_playlistSource[index], _playlistSource);
    }

    public void SkipToPrevSong()
    {
        if (_playlistSource.Count == 0 || CurrentSong == null)
            return;

        int index = _playlistSource.IndexOf(CurrentSong) - 1;
        if (index < 0)
        {
            if (IsLooping)
                index = _playlistSource.Count - 1;
            else
            {
                _player.StopAll();
                return;
            }
        }

        PlayFrom(_playlistSource[index], _playlistSource);
    }

    public void ToggleLoop()
    {
        IsLooping = !IsLooping;
        if (CurrentSong == null) return;
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
        var startIndex = _playlistSource.IndexOf(song);
        var ordered = _playlistSource.Skip(startIndex).Concat(_playlistSource.Take(IsLooping ? startIndex : 0));
        Queue = new ObservableQueue<Song>(ordered);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}