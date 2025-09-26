using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Core;
using Riffle.Core.Audio;

namespace Riffle.Core;

public class QueuePlayer
{
    private readonly IAudioPlayer _player;
    public ObservableQueue<Song> Queue = new();
    public List<Song> PlaylistSource = new();
    public Song CurrentSong { get; private set; }
    public bool Loop { get; private set; }

    public QueuePlayer(IAudioPlayer player)
    {
        _player = player;
        _player.TrackEnded += OnTrackEnded;
    }

    private void OnTrackEnded(object sender, EventArgs e)
    {
        SkipToNextSong();
    }

    public void PlayFrom(Song song, List<Song> playlist) // TODO: work with id instead of whole playlist
    {
        if (song == null || playlist == null || !playlist.Contains(song)) return;

        PlaylistSource = playlist.ToList();

        RecreateQueue(song);

        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);
        
        /*// Build the queue starting from the selected song to the end
        Queue = new ObservableQueue<Song>(playlist);
        int currentIndex = playlist.IndexOf(song);
        for (int i = 0; i < currentIndex; i++)
        {
            Song notPlayed = Queue[i];
            Queue.RemoveAt(i);
            Queue.Add(notPlayed);
        }
        CurrentSong = Queue.First();
        _player.Play(CurrentSong);*/
    }

    public void SkipToNextSong()
    {
        if (Queue.Count == 0)
        {
            _player.Stop();
            return;
        }

        // Dequeue current song
        var prevSong = Queue.Dequeue();
        if (Loop)
        {
            Queue.Enqueue(prevSong);
        }

        /*if (Queue.Count == 0)
        {
            if (Loop)
            {
                ResetQueueFromSource();
            }
            else
            {
                _player.Stop();
                return;
            }
        }*/

        CurrentSong = Queue.Peek();
        _player.Play(CurrentSong);
        
        /*int currentIndex = Queue.IndexOf(CurrentSong);
        currentIndex++;

        if (currentIndex >= Queue.Count)
        {
            if (Loop)
            {
                currentIndex = 0;
            }
            else
            {
                _player.Stop();
                return;
            }
        }

        CurrentSong = Queue[currentIndex];
        _player.Play(CurrentSong);*/
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
                _player.Stop();
                return;
            }
        }

        PlayFrom(PlaylistSource[index], PlaylistSource);
        
        /*int currentIndex = Queue.IndexOf(CurrentSong);
        currentIndex--;

        if (currentIndex < 0)
        {
            if (Loop)
            {
                currentIndex = Queue.Count - 1;
            }
            else
            {
                _player.Stop();
                return;
            }
        }

        CurrentSong = Queue[currentIndex];
        _player.Play(CurrentSong);*/
    }
    
    private void ResetQueueFromSource()
    {
        Queue = new ObservableQueue<Song>(PlaylistSource);
    }

    public void ToggleLoop()
    {
        Loop = !Loop;
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