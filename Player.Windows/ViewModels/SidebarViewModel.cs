using System;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Core.Models;
using Riffle.Player.Windows.Services;

#nullable enable
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

    public PlaylistViewModel AddPlaylist(Playlist playlist)
    {
        var newPlaylist = new PlaylistViewModel(playlist.Name, playlist);
        Playlists.Add(newPlaylist);
        return newPlaylist;
    }
    
    public void RefreshPlaylists()
    {
        Playlists.Clear();
        LoadPlaylists();
    }

    public PlaylistViewModel? GetPlaylist(Guid id)
    {
        //if (id == Guid.Empty) return Playlists.First(pl => pl.Name == "All Songs");
        return Playlists.FirstOrDefault(pl => IsWantedPlaylist(pl, id));
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

    public void RemovePlaylist(PlaylistViewModel selectedVmPlaylist)
    {
        Playlists.Remove(selectedVmPlaylist);
    }

    public PlaylistViewModel GetAllSongsPlaylist()
    {
        var allSongs = GetPlaylist(Guid.Empty);
        allSongs ??= Playlists.FirstOrDefault(pl => pl.Playlist == null);
        allSongs ??= Playlists.FirstOrDefault(pl => pl.Name == "All Songs");

        if (allSongs == null) throw new NullReferenceException("All Songs not found.");
        
        return allSongs;
    }
}