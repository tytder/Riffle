using System;
using System.Collections.ObjectModel;
using System.Linq;
using Riffle.Core.Models;
using Riffle.Data;

#nullable enable
namespace Riffle.Player.Windows.ViewModels;

public class SidebarViewModel
{
    private readonly ILibraryManager _libraryManager;

    public ObservableCollection<PlaylistViewModel> Playlists { get; } = new();

    public SidebarViewModel(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
        LoadPlaylists();
    }

    private void LoadPlaylists()
    {
        // Add real playlists from DB
        var playlists = _libraryManager.GetAllPlaylists();
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
        return Playlists.FirstOrDefault(pl => pl.Playlist.Id == id);
    }

    public void RemovePlaylist(PlaylistViewModel selectedVmPlaylist)
    {
        Playlists.Remove(selectedVmPlaylist);
    }

    public PlaylistViewModel GetAllSongsPlaylist()
    {
        var allSongs = GetPlaylist(_libraryManager.AllSongsPlaylistId);
        if (allSongs == null)
        {
            var allSongsPlaylist = _libraryManager.EnsureAllSongsPlaylistAsync().Result;
            allSongs = new PlaylistViewModel(allSongsPlaylist.Name, allSongsPlaylist);
            Playlists.Add(allSongs);
        } 
        
        return allSongs;
    }
}