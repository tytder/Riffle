using System.Collections.ObjectModel;

namespace Riffle.Player.Windows;

public class SidebarViewModel
{
    private readonly MusicService _musicService;

    public ObservableCollection<PlaylistViewModel> Playlists { get; } = new();

    public SidebarViewModel(MusicService musicService)
    {
        _musicService = musicService;
        LoadPlaylists();
    }

    private void LoadPlaylists()
    {
        // Add special "All Songs" entry
        Playlists.Add(new PlaylistViewModel("All Songs", null));

        // Add real playlists from DB
        var playlists = _musicService.GetAllPlaylists();
        foreach (var p in playlists)
        {
            Playlists.Add(new PlaylistViewModel(p.Name, p));
        }
    }
    
    public void RefreshPlaylists()
    {
        Playlists.Clear();
        LoadPlaylists();
    }
}