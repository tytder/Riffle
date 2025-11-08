using System;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Player.Windows.Services;

namespace Riffle.Player.Windows.ViewModels;

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

    public PlaylistViewModel GetPlaylist(Guid id)
    {
        //if (id == Guid.Empty) return Playlists.First(pl => pl.Name == "All Songs");
        return Playlists.First(pl => IsWantedPlaylist(pl, id));
    }

    private bool IsWantedPlaylist(PlaylistViewModel playlist, Guid wantedId)
    {
        if (playlist.Playlist == null)
        {
            // if playlist was null (normally only "All Songs" playlist)
            // and the wanted id is empty (also normally only "All Songs" playlist)
            // then the current playlist is the wanted one, return true.
            if (wantedId == Guid.Empty) return true; 
            
            // else the "All Songs" playlist is not the wanted playlist, return false.
            return false;
        }
        
        // simply check the id if the "All Songs" playlist is not the wanted playlist.
        return playlist.Playlist.Id == wantedId;
    }
}