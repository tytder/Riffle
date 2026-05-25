using System;
using Player.Desktop.Models;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Services;
using Riffle.Data;

namespace Player.Desktop.ViewModels;

public class DesignerMainWindowViewModel : MainWindowViewModel
{
    public DesignerMainWindowViewModel(
        ILibraryManager fakeLibraryManager,
        IAudioPlayer audioPlayer) : base(
            fakeLibraryManager, 
            audioPlayer,
            new SidebarViewModel(fakeLibraryManager),
            new SongsViewModel(fakeLibraryManager),
            new PlaybackManager(audioPlayer))
    {
        fakeLibraryManager.Initialize();
        
        var pl = fakeLibraryManager.CreatePlaylist("Playlist 1");
        fakeLibraryManager.CreatePlaylist("Playlist 2");
        fakeLibraryManager.CreatePlaylist("Playlist 3");

        var moon = new Song(
            "Moon",
            "Lieless",
            new TimeSpan(0,5,5),
            "/home/matis/Music/Lieless - Moon.mp3");
        var moonP = fakeLibraryManager.AddNewSongToPlaylist(moon, pl.Id);
        var what = new Song(
            "What u Wanna Do (DJ G2G Club Edit)",
            "Erika de Casier",
            new TimeSpan(0,5,30),
            "/home/matis/Music/Erika de Casier - What u Wanna Do_ (DJ G2G Club Edit).mp3");
        var whatP = fakeLibraryManager.AddNewSongToPlaylist(what, pl.Id);
        var inf = new Song(
            "Infinity (Original Mix)",
            "Infinity Ink",
            new TimeSpan(0,5,9),
            "/home/matis/Music/Infinity Ink - Infinity (Original Mix).mp3");
        var infP = fakeLibraryManager.AddNewSongToPlaylist(inf, pl.Id);
        var rain = new Song(
            "Raindance",
            "Vairo",
            new TimeSpan(0,3,7),
            "/home/matis/Music/Vairo - Raindance (Official Music Video).mp3");
        var rainP = fakeLibraryManager.AddNewSongToPlaylist(rain, pl.Id);
        var tues = new Song(
            "Tuesday",
            "Burak Yeter ft. Danelle Sandoval",
            new TimeSpan(0,3,10),
            "/home/matis/Music/Burak Yeter ft. Danelle Sandoval - Tuesday.mp3");
        var tuesP = fakeLibraryManager.AddNewSongToPlaylist(tues, pl.Id);
        var orange = new Song(
            "Trumpsta (Djuro Remix)",
            "Contiez Feat. Treyy G",
            new TimeSpan(0,4,20),
            "/home/matis/Music/Contiez Feat. Treyy G - Trumpsta (Djuro Remix) [Safari Music].mp3");
        var orangeP = fakeLibraryManager.AddNewSongToPlaylist(orange, pl.Id);
        
        PlaybackManager.RecentlyPlayed.Enqueue(new SongPlayed(rainP, DateTime.UtcNow));
        PlaybackManager.RecentlyPlayed.Enqueue(new SongPlayed(infP, DateTime.UtcNow));
        PlaybackManager.RecentlyPlayed.Enqueue(new SongPlayed(moonP, DateTime.UtcNow));
        PlaybackManager.RecentlyPlayed.Enqueue(new SongPlayed(whatP, DateTime.UtcNow));
    }

    public DesignerMainWindowViewModel() : this(new FakeLibraryManager(), new DummyAudioPlayer())
    {
        
    }
}