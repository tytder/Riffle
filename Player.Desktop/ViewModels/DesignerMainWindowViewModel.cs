using System;
using Player.Desktop.Models;
using Riffle.Core.Services;
using Riffle.Data;

namespace Player.Desktop.ViewModels;

public class DesignerMainWindowViewModel : MainWindowViewModel
{
    public DesignerMainWindowViewModel(ILibraryManager fakeLibraryManager) : 
        base(
            fakeLibraryManager, 
            new DummyAudioPlayer(),
            new SidebarViewModel(fakeLibraryManager),
            new SongsViewModel(fakeLibraryManager))
    {
        fakeLibraryManager.Initialize();
        
        var pl = fakeLibraryManager.CreatePlaylist("Playlist 1");
        fakeLibraryManager.CreatePlaylist("Playlist 2");
        fakeLibraryManager.CreatePlaylist("Playlist 3");
        
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "Moon",
                "Lieless",
                new TimeSpan(0,5,5),
                "/home/matis/Music/Lieless - Moon.mp3"),
            pl.Id);
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "What u Wanna Do (DJ G2G Club Edit)",
                "Erika de Casier",
                new TimeSpan(0,5,30),
                "/home/matis/Music/Erika de Casier - What u Wanna Do_ (DJ G2G Club Edit).mp3"),
            pl.Id);
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "Infinity (Original Mix)",
                "Infinity Ink",
                new TimeSpan(0,5,9),
                "/home/matis/Music/Infinity Ink - Infinity (Original Mix).mp3"),
            pl.Id);
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "Raindance",
                "Vairo",
                new TimeSpan(0,3,7),
                "/home/matis/Music/Vairo - Raindance (Official Music Video).mp3"),
            pl.Id);
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "Tuesday",
                "Burak Yeter ft. Danelle Sandoval",
                new TimeSpan(0,3,10),
                "/home/matis/Music/Burak Yeter ft. Danelle Sandoval - Tuesday.mp3"),
            pl.Id);
        fakeLibraryManager.AddNewSongToPlaylist(
            new(
                "Trumpsta (Djuro Remix)",
                "Contiez Feat. Treyy G",
                new TimeSpan(0,4,20),
                "/home/matis/Music/Contiez Feat. Treyy G - Trumpsta (Djuro Remix) [Safari Music].mp3"),
            pl.Id);
    }

    public DesignerMainWindowViewModel() : this(new FakeLibraryManager())
    {
        
    }
}