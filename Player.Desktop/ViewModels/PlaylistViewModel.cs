using Riffle.Core.Models;

namespace Player.Desktop.ViewModels;

#nullable enable
public class PlaylistViewModel
{
    public string Name { get; }
    public Playlist Playlist { get; } 
    public PlaylistViewModel(string name, Playlist playlist)
    {
        Name = name;
        Playlist = playlist;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not PlaylistViewModel other)    return false;   // if other isn't a PlaylistViewModel, return false
        if (ReferenceEquals(this, other))          return true;    // if both references are the same, return true
        return Playlist.Equals(other.Playlist);                    // lastly, check if the playlist id's match
    }

    public override int GetHashCode()
    {
        return Playlist.GetHashCode();
    }
}