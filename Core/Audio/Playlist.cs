using System.Collections.ObjectModel;

namespace Riffle.Core.Audio;

public class Playlist
{
    public Playlist(string name)
    {
        Name = name;
        Id = Guid.NewGuid();
    }

    public string Name { get; }
    public Guid Id { get; }
    
    public ObservableCollection<Song> PlaylistItems { get; set; } = new ObservableCollection<Song>();

    public override bool Equals(object? obj)
    {
        return obj is Playlist other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}