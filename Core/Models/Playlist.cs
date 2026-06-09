using System.Collections.ObjectModel;

namespace Riffle.Core.Models;

public class Playlist
{
    public Playlist()
    {
        
    }
 
    public Playlist(string name) : this (name, false)
    {
    }


    public Playlist(string name, bool isAllSongs)
    {
        Name = name;
        IsAllSongs = isAllSongs;
        LastPlayed = DateTime.UtcNow;
    }

    public string Name { get; private set; } = "New Playlist";
    public Guid Id { get; private set; }
    public bool IsAllSongs { get; private set; }
    public DateTime LastPlayed { get; private set; }

    public ICollection<PlaylistSong> PlaylistItems{ get; private set; }
        = new ObservableCollection<PlaylistSong>();
    
    public override bool Equals(object? obj)
    {
        return obj is Playlist other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}