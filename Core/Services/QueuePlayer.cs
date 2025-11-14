using System.Collections.ObjectModel;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Utilities;

namespace Riffle.Core.Services;

public class QueuePlayer
{
    private readonly IAudioPlayer _player;
    public ObservableQueue<Song> Queue = new();
    public List<Song> PlaylistSource = new();
    public Song? CurrentSong { get; private set; }
    public bool Loop { get; private set; }

    public QueuePlayer(IAudioPlayer player)
    {
        _player = player;
        _player.TrackEnded += PlayerOnTrackEnded;
    }

    public void PlayFrom(Song song, List<Song> playlist) // TODO: work with id instead of whole playlist
    {
        PlaylistSource = playlist.ToList();

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
        var prevSong = Queue.Dequeue();
        if (Loop)
        {
            Queue.Enqueue(prevSong);
        }
        
        if (Queue.Count == 0)
        {
            _player.StopAll();
            return;
        }
        
        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);
    }

    public void SkipToPrevSong()
    {
        if (PlaylistSource.Count == 0 || CurrentSong == null)
            return;

        int index = PlaylistSource.IndexOf(CurrentSong) - 1;
        if (index < 0)
        {
            if (Loop)
                index = PlaylistSource.Count - 1;
            else
            {
                _player.StopAll();
                return;
            }
        }

        PlayFrom(PlaylistSource[index], PlaylistSource);
    }
    
    private void ResetQueueFromSource()
    {
        Queue = new ObservableQueue<Song>(PlaylistSource);
    }

    public void ToggleLoop()
    {
        Loop = !Loop;
        if (CurrentSong == null) return;
        var startIndex = PlaylistSource.IndexOf(CurrentSong);
        for (var index = 0; index < PlaylistSource.Count; index++)
        {
            if (Loop)
            {
                var playlistSong = PlaylistSource[(startIndex + index) % PlaylistSource.Count];
                if (Queue.Contains(playlistSong)) continue;
                Queue.Enqueue(playlistSong);
            }
            else
            {
                if (index >= startIndex) break;
                var playlistSong = PlaylistSource[index];
                if (!Queue.Contains(playlistSong)) continue;
                Queue.Remove(playlistSong);
            }
        }
    }

    private void RecreateQueue(Song song)
    {
        var startIndex = PlaylistSource.IndexOf(song);
        var ordered = PlaylistSource.Skip(startIndex).Concat(PlaylistSource.Take(Loop ? startIndex : 0));
        Queue = new ObservableQueue<Song>(ordered);
    }
}