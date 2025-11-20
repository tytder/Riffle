using Riffle.Core.Models;

namespace Riffle.Core.CustomEventArgs;

public class TrackEventArgs : EventArgs
{
    public Song Song { get; }

    public TrackEventArgs(Song song)
    {
        Song = song;
    }
}