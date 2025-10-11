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
}