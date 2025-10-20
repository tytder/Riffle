using System;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

#nullable enable
public class PlaylistViewModel
{
    public string Name { get; }
    public Playlist? Playlist { get; } // null means "All Songs"

    public PlaylistViewModel(string name, Playlist? playlist)
    {
        Name = name;
        Playlist = playlist;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not PlaylistViewModel other) return false; // if other isnt a PlaylistViewModel, return false
        if (ReferenceEquals(this, other)) return true; // if both references are the same, return true
        if (Playlist is null && other.Playlist is null) return true; // if both playlists are null, return true (both "All Songs" playlist)
        if (Playlist is null || other.Playlist is null) return false; // if only one of the two is null, return false
        return  Playlist.Equals(other.Playlist); // lastly, check if the playlist id's match
    }

    public override int GetHashCode()
    {
        if (Playlist == null) return Guid.Empty.GetHashCode();
        return Playlist.GetHashCode();
    }
}