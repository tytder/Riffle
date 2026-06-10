using System.Collections.ObjectModel;
using Riffle.Core.Utilities;

namespace Riffle.Core.Models;

public class Playlist
{
    public Playlist()
    {
        
    }
    
    public Playlist(string name, bool isAllSongs = false)
    {
        Name = name;
        IsAllSongs = isAllSongs;
        LastPlayed = DateTime.UtcNow;
    }

    public string Name { get; private set; } = "New Playlist";
    public Guid Id { get; private set; }
    public bool IsAllSongs { get; private set; }
    public DateTime LastPlayed { get; private set; }

    public virtual ObservableCollection<PlaylistSong> PlaylistItems{ get; private set; }
        = new ObservableCollection<PlaylistSong>();
    
    public override bool Equals(object? obj)
    {
        return obj is Playlist other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public class QueuePlaylist : Playlist
{
    public QueuePlaylist() : base() { }
    public QueuePlaylist(string name) : base(name) { }
    
    public ObservableQueue<PlaylistSong> QueuePlaylistItems { get; private set; }
        = new ObservableQueue<PlaylistSong>();

    public override ObservableCollection<PlaylistSong> PlaylistItems => QueuePlaylistItems;
}