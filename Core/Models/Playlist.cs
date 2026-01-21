using System.Collections.ObjectModel;

namespace Riffle.Core.Models;

public class Playlist
{
    public Playlist(string name)
    {
        Name = name;
    }

    public Playlist(string name, bool isAllSongs)
    {
        Name = name;
        IsAllSongs = isAllSongs;
    }

    public string Name { get; private set; }
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsAllSongs { get; private set; }

    public ICollection<PlaylistSong> PlaylistItems{ get; private set; }
        = new ObservableCollection<PlaylistSong>();
    
    public override bool Equals(object? obj)
    {
        return obj is Playlist other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}