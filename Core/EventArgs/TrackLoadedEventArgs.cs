using Riffle.Core.Models;

namespace Riffle.Core.Interfaces;

public class TrackLoadedEventArgs : EventArgs
{
    public Song SongLoaded { get; }

    public TrackLoadedEventArgs(Song songLoaded)
    {
        SongLoaded = songLoaded;
    }
}