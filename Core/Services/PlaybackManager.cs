#nullable enable
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Utilities;

namespace Riffle.Core.Services;

public class PlaybackManager
{
    private readonly QueuePlayer _queuePlayer;
    public ObservableQueue<Song> Queue => _queuePlayer.Queue;
    public ObservableQueue<Song> RecentlyPlayed;
    
    public bool IsLooping => _queuePlayer.Loop; // TODO: should be in here instead?
    
    public PlaybackManager(IAudioPlayer audioPlayer)
    {
        _queuePlayer = new QueuePlayer(audioPlayer);
        RecentlyPlayed = new ObservableQueue<Song>(50, true);
        audioPlayer.TrackEnded += AudioPlayerOnTrackEnded;
    }

    private void AudioPlayerOnTrackEnded(object? sender, TrackEventArgs e)
    {
        RecentlyPlayed.Enqueue(e.Song);
    }

    public void PlayFrom(Song? selectedSong, List<Song> playListSongs)
    {
        if (selectedSong == null || !playListSongs.Contains(selectedSong)) return;
        _queuePlayer.PlayFrom(selectedSong, playListSongs);
    }

    public void ToggleLoop()
    {
        _queuePlayer.ToggleLoop();
    }

    public void SkipToNextSong()
    {
        _queuePlayer.SkipToNextSong();
    }

    public void SkipToPrevSong()
    {
        _queuePlayer.SkipToPrevSong();
    }
}