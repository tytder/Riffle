using Riffle.Core.Models;

namespace Riffle.Core.CustomEventArgs;

public class PlaylistEventArgs : EventArgs
{
    public Playlist? Playlist { get; }

    public PlaylistEventArgs(Playlist? playlist)
    {
        Playlist = playlist;
    }
}