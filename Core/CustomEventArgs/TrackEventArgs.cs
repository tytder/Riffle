using Riffle.Core.Models;

namespace Riffle.Core.CustomEventArgs;

public class TrackEventArgs : EventArgs
{
    public PlaylistSong PlaylistSong { get; }

    public TrackEventArgs(PlaylistSong playlistSong)
    {
        PlaylistSong = playlistSong;
    }
}