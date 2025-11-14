using Riffle.Core.Models;

namespace Riffle.Core.Interfaces;

public class TrackEventArgs : EventArgs
{
    public Song Song { get; }

    public TrackEventArgs(Song song)
    {
        Song = song;
    }
}