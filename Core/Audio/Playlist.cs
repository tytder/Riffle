using System.Collections.ObjectModel;

namespace Riffle.Core.Audio;

public class Playlist
{
    public Playlist(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = "";
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public ObservableCollection<Song> PlaylistItems { get; private set; } = new ObservableCollection<Song>();

    public override bool Equals(object? obj)
    {
        return obj is Playlist other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}