using System;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class QueuePlayer
{
    private readonly IAudioPlayer _player;
    public ObservableCollection<Song> QueueCollection = new();
    public Song CurrentSong;
    public bool Loop;

    public QueuePlayer(IAudioPlayer player)
    {
        _player = player;
        _player.TrackEnded += OnTrackEnded;
    }

    private void OnTrackEnded(object sender, EventArgs e)
    {
        SkipToNextSong();
    }

    public void PlayFrom(Song song, ObservableCollection<Song> playlist)
    {
        if (song == null || playlist == null || !playlist.Contains(song)) return;

        // Build the queue starting from the selected song to the end
        QueueCollection = new ObservableCollection<Song>(playlist);
        int currentIndex = playlist.IndexOf(song);
        for (int i = 0; i < currentIndex; i++)
        {
            Song notPlayed = QueueCollection[i];
            QueueCollection.RemoveAt(i);
            QueueCollection.Add(notPlayed);
        }
        CurrentSong = QueueCollection.First();
        _player.Play(CurrentSong);
    }

    public void SkipToNextSong()
    {
        int currentIndex = QueueCollection.IndexOf(CurrentSong);
        currentIndex++;

        if (currentIndex >= QueueCollection.Count)
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

        CurrentSong = QueueCollection[currentIndex];
        _player.Play(CurrentSong);
    }

    public void SkipToPrevSong()
    {
        int currentIndex = QueueCollection.IndexOf(CurrentSong);
        currentIndex--;

        if (currentIndex < 0)
        {
            if (Loop)
            {
                currentIndex = QueueCollection.Count - 1;
            }
            else
            {
                _player.Stop();
                return;
            }
        }

        CurrentSong = QueueCollection[currentIndex];
        _player.Play(CurrentSong);
    }
}