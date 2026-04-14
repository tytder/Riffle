#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Player.Desktop.Models;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Models;
using Riffle.Core.Utilities;
using Player.Desktop.ViewModels;
using Player.Desktop.Views;
using Riffle.Core.Interfaces;
using Riffle.Core.Services;
using Riffle.Data;
using TagLib;
using File = TagLib.File;

namespace Player.Desktop;

public partial class MainWindow : Window
{
    private readonly IAudioPlayer _player;

    private readonly SolidColorBrush _buttonInactiveBrush;
    private readonly SolidColorBrush _buttonActiveBrush;

    private readonly Dictionary<double, double[]> _songColumnPresets = new()
    {
        { 143, new[] { 1d, 0d, 0d } },
        { 330, new[] { 0.6, 0d, 0.4 } },
        { 500, new[] { 0.50, 0.30, 0.2 } },
    };

    public MainWindowViewModel ViewModel { get; }

    private bool _isDraggingBar;
    private bool _seekBarWasRecentlyAutoUpdated;

    public MainWindow()
    {
        if (!Design.IsDesignMode) throw new NotSupportedException("Parameterless ctor called while not in designer mode is not intended!");
        
        InitializeComponent();

        _player = new DummyAudioPlayer();
        _player.PlayingStateChanged += PlayerOnPlayingStateChanged;
        ViewModel = new DesignerMainWindowViewModel();

        PlaylistList.SelectedIndex = 0;
        PlaylistList.ItemsSource = ViewModel.SidebarViewModel.Playlists;
        ViewModel.SidebarViewModel.RefreshPlaylists();
        DataContext = ViewModel;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        timer.Tick += Timer_Tick;
        timer.Start();

        _player.TrackLoaded += Player_TrackLoaded;
        _player.StopAllCalled += OnStopCalled;

        var buttonInactiveColor = Color.FromRgb(80, 80, 80);
        _buttonInactiveBrush = new SolidColorBrush(buttonInactiveColor);
        _buttonActiveBrush = new SolidColorBrush(Colors.White);

        BtnLoop.Background = _buttonInactiveBrush;
        BtnShuffle.Background = _buttonInactiveBrush;
        BtnShuffleHeader.Background = _buttonInactiveBrush;
    }
    
    public MainWindow(
        MainWindowViewModel vm,
        IAudioPlayer player)
    {
        InitializeComponent();

        _player = player;
        _player.PlayingStateChanged += PlayerOnPlayingStateChanged;
        ViewModel = vm;

        PlaylistList.SelectedIndex = 0;
        DataContext = ViewModel;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        timer.Tick += Timer_Tick;
        timer.Start();

        _player.TrackLoaded += Player_TrackLoaded;
        _player.StopAllCalled += OnStopCalled;
        
        var buttonInactiveColor = Color.FromRgb(80, 80, 80);
        _buttonInactiveBrush = new SolidColorBrush(buttonInactiveColor);
        _buttonActiveBrush = new SolidColorBrush(Colors.White);

        BtnLoop.Background = _buttonInactiveBrush;
        BtnShuffle.Background = _buttonInactiveBrush;
        BtnShuffleHeader.Background = _buttonInactiveBrush;
    }

    private void PlayerOnPlayingStateChanged(object? sender, PlayingStateEventArgs e)
    {
        SetPauseResume(e.IsPlaying);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _player.Dispose();
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void SeekBar_PointerReleased(object? sender, PointerCaptureLostEventArgs pointerCaptureLostEventArgs)
    {
        /*if (!_isDraggingSeekBar)
            return;

        _isDraggingSeekBar = false;
        e.Pointer.Capture(null);
        */

        _isDraggingBar = false;
        _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
    }

    private void Player_TrackLoaded(object? sender, TrackEventArgs e)
    {
        TxtTotalTime.Text = e.Song.Duration.TotalSeconds.ToMmSs();
        SeekBar.Maximum = e.Song.Duration.TotalSeconds;
        SeekBar.Value = 0;
        TxtSongTitle.Text = e.Song.Title;
        TxtArtistName.Text = e.Song.Artist;
        _isDraggingBar = false;

        QueueListView.ItemsSource = ViewModel.TotalQueue;
        RecentlyPlayedListView.ItemsSource = ViewModel.RecentlyPlayed;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_player.HasTrackLoaded)
            return;
        if (_isDraggingBar)
            return;
        
        _seekBarWasRecentlyAutoUpdated = true;
        SeekBar.Value = _player.CurrentTime.TotalSeconds;
        TxtCurrentTime.Text = SeekBar.Value.ToMmSs();
    }

    private void OnPauseResume(object? sender, RoutedEventArgs e)
    {
        if (!_player.HasTrackLoaded)
            return;

        _player.TogglePlaying();
    }

    private void SetPauseResume(bool isPlaying)
    {
        SetControlBarPlaying(isPlaying);
        SetPlaylistHeaderPlaying(isPlaying);
    }

    private void SetPlaylistHeaderPlaying(bool isPlaying, PlaylistViewModel? playlistViewModel = null)
    {
        playlistViewModel ??= PlaylistList.SelectedItem as PlaylistViewModel;

        if (isPlaying && Equals(ViewModel.CurrentPlaylistPlaying, playlistViewModel))
        {
            PlaylistPlayBtn.Content = "⏸";
            PlaylistPlayBtn.Padding = new Thickness(-2, -2, -2, -0);
            PlaylistPlayBtn.FontSize = 18;
        }
        else
        {
            PlaylistPlayBtn.Content = "▶";
            PlaylistPlayBtn.Padding = new Thickness(5, -1, 5, 0);
            PlaylistPlayBtn.FontSize = 12;
        }
    }

    private void SetControlBarPlaying(bool isPlaying)
    {
        if (!isPlaying)
        {
            BtnPauseResume.Content = "▶";
            BtnPauseResume.Padding = new Thickness(-2, -3.5, -2, -0.5);
            BtnPauseResume.FontSize = 16;
        }
        else
        {
            BtnPauseResume.Content = "⏸";
            BtnPauseResume.Padding = new Thickness(-2.5, -3.3, -2, -0.5);
            BtnPauseResume.FontSize = 20.2;
        }
    }

    // Keep this wired in XAML as ValueChanged="SeekBar_ValueChanged" if you want teleport behavior.
    // Depending on Avalonia version, you might need to change the event type to a concrete ValueChanged args.
    private void SeekBar_ValueChanged(object? sender, RoutedEventArgs e)
    {
        if (!_seekBarWasRecentlyAutoUpdated)
        {
            _isDraggingBar = true;
        }

        //double totalSeconds;
        if (_player.HasTrackLoaded)
            TxtCurrentTime.Text = SeekBar.Value.ToMmSs();
            /*
        {
            if (!_isDraggingBar)
            {
                totalSeconds = _player.CurrentTime.TotalSeconds;
            }
            else
            {
                totalSeconds = SeekBar.Maximum;
            }
        }
            */

        _seekBarWasRecentlyAutoUpdated = false;
    }

    private void VolumeBar_ValueChanged(object? sender, RoutedEventArgs e)
    {
        if (!IsVisible)
            return;

        _player.SetVolume((float)VolumeBar.Value / 100);
        TxtVolumePercentage.Text = (int)VolumeBar.Value + "%";
    }
    
    public static FilePickerFileType AudioAll { get; } = new("Audio files")
    {
        Patterns = new[] { "*.mp3", "*.wav", "*.m4a", "*.flac", "*.opus", "*.ogg" },
    };

    // Import songs – converted to Avalonia's OpenFileDialog (async)
    private async void BtnImportSong_OnClick(object? sender, RoutedEventArgs e)
    {
        // Rough scrollbar width approximation in place of SystemParameters.VerticalScrollBarWidth
        const double scrollBarWidth = 18;

        var totalWidth = PlaylistContent.Bounds.Width
                         - scrollBarWidth
                         /*- GridView.Columns[0].Width
                         - GridView.Columns[^1].Width*/
                         - PlaylistContent.Padding.Left
                         - PlaylistContent.Padding.Right;

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                AllowMultiple = true,
                FileTypeFilter = [AudioAll],
            });

        if (files is null || files.Count == 0)
            return;

        foreach (var file in files)
        {
            var uri = file.Path.LocalPath;
            ShowSongMetadataDialog(uri);
        }
    }

    private async void ShowSongMetadataDialog(string filePath)
    {
        var file = new File.LocalFileAbstraction(filePath);
        var tagFile = TagLib.File.Create(file);

        var suggestedTitle = !string.IsNullOrEmpty(tagFile.Tag.Title)
            ? tagFile.Tag.Title
            : System.IO.Path.GetFileNameWithoutExtension(filePath);

        var suggestedArtist = tagFile.Tag.Performers is { Length: > 0 }
            ? tagFile.Tag.Performers[0]
            : string.Empty;

        var metadataWindow = new SongImportData()
        {
            FilePath = System.IO.Path.GetFileName(filePath),
            TxtSongTitle = { Text = suggestedTitle },
            TxtArtistName = { Text = suggestedArtist }
        };

        var result = await metadataWindow.ShowDialog<bool?>(this);
        if (result != true)
            return;

        var title = metadataWindow.SongTitle;
        var artist = metadataWindow.ArtistName;
        
        var duration = tagFile.Properties.Duration;

        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        var playlist = selectedVm.Playlist;

        var newSong = new Song(title, artist, duration, filePath);
        var newPlaylistSong = ViewModel.AddSong(newSong, playlist);

        if (_player.IsPlaying)
            return;

        ViewModel.PlayFrom(selectedVm, newPlaylistSong);
    }

    private void PlaylistContent_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SetSongGridColumnWidths();
    }

    private void SetSongGridColumnWidths()
    {
        /*if (GridView.Columns.Count < 2)
            return;*/

        const double scrollBarWidth = 18;

        var totalWidth =
            PlaylistContent.Bounds.Width
            - scrollBarWidth
            /*- GridView.Columns[0].Width
            - GridView.Columns[^1].Width*/
            - PlaylistContent.Padding.Left;

        if (totalWidth <= 0)
            return;

        var ratios = GetPresetForWidth(totalWidth);

        /*for (var i = 0; i < GridView.Columns.Count - 2 && i < ratios.Length; i++)
        {
            var ratio = ratios[i];

            var col = GridView.Columns[i + 1];
            col.Width = ratio <= 0 ? 0 : totalWidth * ratio;
        }*/
    }

    private double[] GetPresetForWidth(double totalWidth)
    {
        var thresholds = _songColumnPresets.Keys.OrderBy(t => t).ToArray();

        var chosenThreshold = thresholds[0];
        foreach (var t in thresholds)
        {
            if (totalWidth >= t)
                chosenThreshold = t;
            else
                break;
        }

        return _songColumnPresets[chosenThreshold];
    }

    private void PlaylistContent_OnMouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        if (PlaylistContent.SelectedItem is not PlaylistSong selectedSong)
            return;

        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        ViewModel.PlayFrom(selectedVm, selectedSong);
    }

    private void PlaylistList_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        const double scrollBarWidth = 18;
        var totalWidth = PlaylistContent.Bounds.Width - scrollBarWidth;
        if (totalWidth > 0) PlaylistList.Width = totalWidth * 4 / 12;
        /*if (PlaylistView.Columns.Count > 0)
        {
            PlaylistView.Columns[0].Width = totalWidth * 4 / 12;
        }*/
    }

    private void PlaylistList_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        OpenPlaylist(selectedVm);
    }

    private void OpenPlaylist(PlaylistViewModel selectedVm)
    {
        ViewModel.SongsViewModel.LoadSongs(selectedVm);
        SetPlaylistHeaderPlaying(_player.IsPlaying, selectedVm);
        PlaylistInfo.Text = ViewModel.SelectedPlaylistInfo;
        PlaylistList.SelectedItem = selectedVm;
    }

    private void PlaylistList_OnMouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        ViewModel.SongsViewModel.RefreshSongs();
        ViewModel.PlayFrom(selectedVm);
    }

    private async void AddPlaylist_Click(object? sender, RoutedEventArgs e)
    {
        var playlistWindow = new NewPlaylistWindow();
        var result = await playlistWindow.ShowDialog<bool>(this);
        if (result)
        {
            PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
        }
        /*playlistWindow.ShowDialog<bool?>(this).ContinueWith(t =>
        {
            if (t.Result == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
                });
            }
        });*/
    }

    private async void RemovePlaylist_Click(object? sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        if (selectedVm.Playlist == null)
            return;

        var deletePlaylistWindow = new DeletePlaylistWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var result = await deletePlaylistWindow.ShowDialog<bool>(this);
        if (result)
        {
            ViewModel.DeletePlaylist(selectedVm);
            ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
        }
        /*deletePlaylistWindow.ShowDialog<bool?>(this).ContinueWith(t =>
        {
            if (t.Result == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ViewModel.DeletePlaylist(selectedVm);
                    ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
                });
            }
        });*/
    }

    private void Queue_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.IsQueueWindowOpen = !ViewModel.IsQueueWindowOpen;

        const double scrollBarWidth = 18;
        var totalWidth = PlaylistContent.Bounds.Width - scrollBarWidth;
        QueueListView.Width = ViewModel.IsQueueWindowOpen ? totalWidth * 5 / 12 : 0;
    }

    private void BtnLoop_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ToggleLoop();
        BtnLoop.Background = ViewModel.IsLooping ? _buttonActiveBrush : _buttonInactiveBrush;
    }

    private void BtnNextSong_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SkipToNextSong();
    }

    private void BtnPreviousSong_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SkipToPrevSong();
    }

    private void OnStopCalled(object? sender, EventArgs e)
    {
        TxtSongTitle.Text = _player.SongTitle;
        TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
        TxtTotalTime.Text = _player.TotalTime.TotalSeconds.ToMmSs();
        TxtArtistName.Text = string.Empty;
        SeekBar.Value = 0;
    }

    private void PlaylistPlayBtn_OnBtnClick(object? sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        if (!Equals(ViewModel.CurrentPlaylistPlaying, selectedVm))
        {
            ViewModel.PlayFrom(selectedVm);
            return;
        }

        _player.TogglePlaying();
    }

    private void ClearQueueClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ClearUserQueue();
    }

    private void GoToCurrentSongPlaylist(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentSong == null)
            return;

        var songPlaylist = ViewModel.GetPlaylistModel(ViewModel.CurrentSong);
        OpenPlaylist(songPlaylist);
    }

    private void GoToCurrentPlaylistPlaying(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPlaylistPlaying == null)
            return;

        OpenPlaylist(ViewModel.CurrentPlaylistPlaying);
    }
}

/*using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Player.Desktop.Models;
using Player.Desktop.ViewModels;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Models;
using Riffle.Core.Utilities;

namespace Player.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly VlcAudioPlayer _player;

    private readonly SolidColorBrush _buttonInactiveBrush;
    private readonly SolidColorBrush _buttonActiveBrush;

    private readonly Dictionary<double, double[]> _songColumnPresets = new Dictionary<double, double[]>()
    {
        { 143, [1, 0, 0] },
        { 330, [.6, 0, .4] },
        { 500, [.50, .30, .2] },
    };

    public MainWindowViewModel ViewModel { get; }

    private bool _isTeleportingSeekBarThumb;

    private bool _seekBarWasRecentlyAutoUpdated;
    private bool _isDraggingSeekBar;

    public MainWindow(
        MainWindowViewModel vm,
        VlcAudioPlayer player
    )
    {
        InitializeComponent();

        _player = player;
        _player.PlayingStateChanged += PlayerOnPlayingStateChanged;
        ViewModel = vm;
        PlaylistList.SelectedIndex = 0;
        DataContext = ViewModel;

        DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
        timer.Tick += Timer_Tick;
        timer.Start();

        _player.TrackLoaded += Player_TrackLoaded;
        _player.StopAllCalled += OnStopCalled;
        Loaded += OnLoaded;

        var buttonInactiveColor = Color.FromRgb(80, 80, 80);
        _buttonInactiveBrush = new SolidColorBrush(buttonInactiveColor);
        _buttonActiveBrush = new SolidColorBrush(Colors.White);

        BtnLoop.Background = _buttonInactiveBrush;
        BtnShuffle.Background = _buttonInactiveBrush;
        BtnShuffleHeader.Background = _buttonInactiveBrush;
        
        //SeekBar.AddHandler(new RoutedEvent("PointerMoved", RoutingStrategies.Tunnel, ));
    }

    private void PlayerOnPlayingStateChanged(object? sender, PlayingStateEventArgs e)
    {
        SetPauseResume(e.IsPlaying);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _player.Dispose();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SeekBar.ApplyTemplate(); // ensure the template is created

        /*SeekBar.AddHandler(
            UIElement.MouseLeftButtonDownEvent,
            new MouseButtonEventHandler(SeekBar_PreviewMouseLeftButtonDown),
            handledEventsToo: true);#1#
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Border_MouseLeftButtonDown(object? sender, PointerPressedEventArgs pointerPressedEventArgs)
    {
        /*if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();#1#
    }

    /*private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var slider = (Slider)sender;
        Point clickPoint = e.GetPosition(slider);

        // Calculate new value
        double ratio = clickPoint.X / slider.Width;
        slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;

        // Start manual drag
        _isDraggingSeekBar = true;
        //Mouse.Capture(slider);
        e.Handled = true;
    }#1#

    // TODO: WAS PREVIEW
    private void SeekBar_PreviewMouseMove(object? sender, PointerEventArgs pointerEventArgs)
    {
        if (!_isDraggingSeekBar) return;
        if (sender is not Slider slider) return;
        Point pos = pointerEventArgs.GetPosition(slider);
        double ratio = pos.X / slider.Width;
        slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;
        if (_player.HasTrackLoaded) TxtCurrentTime.Text = slider.Value.ToMmSs();
    }

    // TODO: WAS PREVIEW
    private void SeekBar_PreviewMouseLeftButtonUp(object? sender, PointerReleasedEventArgs pointerReleasedEventArgs)
    {
        if (!_isDraggingSeekBar) return;
        _isDraggingSeekBar = false;
        _isTeleportingSeekBarThumb = false;
        //Mouse.Capture(null);
        _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
    }

    private void Player_TrackLoaded(object? sender, TrackEventArgs e)
    {
        TxtTotalTime.Text = e.Song.Duration.TotalSeconds.ToMmSs();
        SeekBar.Maximum = e.Song.Duration.TotalSeconds;
        SeekBar.Value = 0;
        TxtSongTitle.Text = e.Song.Title;
        TxtArtistName.Text = e.Song.Artist;
        _isTeleportingSeekBarThumb = false;
        QueueListView.ItemsSource = ViewModel.TotalQueue;
        RecentlyPlayedListView.ItemsSource = ViewModel.RecentlyPlayed;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_player.HasTrackLoaded) return;
        if (_isDraggingSeekBar || _isTeleportingSeekBarThumb) return;
        _seekBarWasRecentlyAutoUpdated = true;
        SeekBar.Value = _player.CurrentTime.TotalSeconds;
    }

    private void OnPauseResume(object sender, RoutedEventArgs e)
    {
        if (!_player.HasTrackLoaded) return;
        _player.TogglePlaying();
    }

    private void SetPauseResume(bool isPlaying)
    {
        SetControlBarPlaying(isPlaying);
        SetPlaylistHeaderPlaying(isPlaying);
    }

    private void SetPlaylistHeaderPlaying(bool isPlaying, PlaylistViewModel? playlistViewModel = null)
    {
        playlistViewModel ??= PlaylistList.SelectedItem as PlaylistViewModel;

        if (isPlaying && Equals(ViewModel.CurrentPlaylistPlaying, playlistViewModel))
        {
            PlaylistPlayBtn.Content = "⏸";
            PlaylistPlayBtn.Padding = new Thickness(-2, -2, -2, -0);
            PlaylistPlayBtn.FontSize = 18;
        }
        else
        {
            PlaylistPlayBtn.Content = "▶";
            PlaylistPlayBtn.Padding = new Thickness(5, -1, 5, 0);
            PlaylistPlayBtn.FontSize = 12;
        }
    }

    private void SetControlBarPlaying(bool isPlaying)
    {
        if (!isPlaying)
        {
            BtnPauseResume.Content = "▶";
            BtnPauseResume.Padding = new Thickness(-2, -3.5, -2, -0.5);
            BtnPauseResume.FontSize = 16;
        }
        else
        {
            BtnPauseResume.Content = "⏸";
            BtnPauseResume.Padding = new Thickness(-2.5, -3.3, -2, -0.5);
            BtnPauseResume.FontSize = 20.2;
        }
    }

    private void SeekBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
    {
        if (!_seekBarWasRecentlyAutoUpdated && !_isDraggingSeekBar)
        {
            _isTeleportingSeekBarThumb = true;
        }

        if (_player.HasTrackLoaded) TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
        _seekBarWasRecentlyAutoUpdated = false;
    }

    private void VolumeBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
    {
        if (!IsLoaded) return;
        _player.SetVolume((float)VolumeBar.Value / 100);
        TxtVolumePercentage.Text = ((int)VolumeBar.Value) + "%";
    }

    private void BtnImportSong_OnClick(object sender, RoutedEventArgs e)
    {
        /*var dialog = new OpenFileDialog
        {
            Filter = "Audio files|*.mp3;*.wav",
            Multiselect = true
        };
        if ((int)dialog.ShowDialog() % 5 != 1) return; // check for any ok sign
        foreach (var file in dialog.FileNames)
        {
            ShowSongMetadataDialog(file);
        }#1#
        // TODO
    }

    /*private void ShowSongMetadataDialog(string filePath)
    {
        var tagFile = TagLib.File.Create(filePath);

        string suggestedTitle = !string.IsNullOrEmpty(tagFile.Tag.Title)
            ? tagFile.Tag.Title
            : System.IO.Path.GetFileNameWithoutExtension(filePath);

        string suggestedArtist = tagFile.Tag.Performers is { Length: > 0 } // null check and length check in 1
            ? tagFile.Tag.Performers[0]
            : string.Empty;

        SongImportData metadataWindow = new SongImportData
        {
            FilePath = System.IO.Path.GetFileName(filePath),
            TxtSongTitle = { Text = suggestedTitle },
            TxtArtistName = { Text = suggestedArtist }
        };

        if (metadataWindow.ShowDialog() != true) return;
        string title = metadataWindow.SongTitle;
        string artist = metadataWindow.ArtistName;
        TimeSpan duration = tagFile.Properties.Duration;

        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;
        // The actual playlist, or null for "All Songs"
        Playlist playlist = selectedVm.Playlist;

        Song newSong = new Song(title, artist, duration, filePath);

        var newPlaylistSong = ViewModel.AddSong(newSong, playlist);

        if (_player.IsPlaying) return; // if no track currently playing, switch to imported song.
        ViewModel.PlayFrom(selectedVm, newPlaylistSong);
    }#1#

    private void PlaylistContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // TODO
        //SetSongGridColumnWidths();
    }

    /*private void SetSongGridColumnWidths()
    {
        if (GridView.Columns.Count < 2) return;

        // Fixed columns: index (0) and duration (last)
        double totalWidth =
            PlaylistContent.Width
            - SystemParameters.VerticalScrollBarWidth
            - GridView.Columns[0].Width // index column
            - GridView.Columns[^1].Width // duration column
            - PlaylistContent.Padding.Left;
        /*
        - PlaylistContent.Padding.Right;
        #2#

        if (totalWidth <= 0) return;

        // Find best preset for current width
        double[] ratios = GetPresetForWidth(totalWidth);

        // 3. Apply ratios to song columns: Title, Artist, DateAdded
        // Assume:
        //   Col 0 = index (fixed)
        //   Col 1 = title
        //   Col 2 = artist
        //   Col 3 = date added
        //   Col 4 = duration (fixed)
        for (int i = 0; i < GridView.Columns.Count - 2 && i <= ratios.Length; i++)
        {
            var ratio = ratios[i];

            if (ratio <= 0)
            {
                // Hide this column by shrinking it;
                // alternatively, you could collapse the header template.
                GridView.Columns[i + 1].Width = 0;
            }
            else
            {
                GridView.Columns[i + 1].Width = totalWidth * ratio;
            }
        }
    }#1#

    private double[] GetPresetForWidth(double totalWidth)
    {
        // Presets sorted by threshold
        var thresholds = _songColumnPresets.Keys.OrderBy(t => t).ToArray();

        double chosenThreshold = thresholds[0];

        foreach (var t in thresholds)
        {
            if (totalWidth >= t)
                chosenThreshold = t;
            else
                break;
        }

        return _songColumnPresets[chosenThreshold];
    }

    private void PlaylistContent_OnMouseDoubleClick(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (PlaylistContent.SelectedItem is not PlaylistSong selectedSong) return;

        // Determine which playlist view-model is currently selected in the sidebar
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;
        ViewModel.PlayFrom(selectedVm, selectedSong);
    }

    private void PlaylistList_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double totalWidth = PlaylistContent.Width - SystemParameters.VerticalScrollBarWidth;

        // TODO
        //PlaylistView.Columns[0].Width = totalWidth * 4 / 12;
    }

    private void PlaylistList_OnSelected(object sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;
        OpenPlaylist(selectedVm);
    }

    private void OpenPlaylist(PlaylistViewModel selectedVm)
    {
        ViewModel.SongsViewModel.LoadSongs(selectedVm);
        SetPlaylistHeaderPlaying(_player.IsPlaying, selectedVm);
        PlaylistInfo.Text = ViewModel.SelectedPlaylistInfo;
        PlaylistList.SelectedItem = selectedVm;
    }

    private void PlaylistList_OnMouseDoubleClick(object? sender, TappedEventArgs tappedEventArgs)
    {
        // Determine which playlist view-model is currently selected in the sidebar
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;
        ViewModel.SongsViewModel.RefreshSongs();
        ViewModel.PlayFrom(selectedVm);
    }

    // TODO
    /*private void AddPlaylist_Click(object sender, RoutedEventArgs e)
    {
        NewPlaylistWindow playlistWindow = new NewPlaylistWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        if (playlistWindow.ShowDialog() != true) return;
        PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
    }#1#

    // TODO
    /*private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;

        // If this is the "All Songs" pseudo-entry, do nothing
        if (selectedVm.Playlist == null) return;

        var deletePlaylistWindow = new DeletePlaylistWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        if (deletePlaylistWindow.ShowDialog() != true) return;

        // Delete from database via service (implement if missing)
        ViewModel.DeletePlaylist(selectedVm);

        ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
    }#1#

    private void Queue_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.IsQueueWindowOpen = !ViewModel.IsQueueWindowOpen;

        double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;
        QueueOverlayColumn.Width = ViewModel.IsQueueWindowOpen ? totalWidth * 5 / 12 : 0;
    }

    private void BtnLoop_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleLoop();
        BtnLoop.Background = ViewModel.IsLooping ? _buttonActiveBrush : _buttonInactiveBrush;
    }

    private void BtnNextSong_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SkipToNextSong();
    }

    private void BtnPreviousSong_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SkipToPrevSong();
    }

    private void OnStopCalled(object? sender, EventArgs eventArgs)
    {
        TxtSongTitle.Text = _player.SongTitle;
        TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
        TxtTotalTime.Text = _player.TotalTime.TotalSeconds.ToMmSs();
        TxtArtistName.Text = "";
        SeekBar.Value = 0;
    }

    private void PlaylistPlayBtn_OnBtnClick(object sender, RoutedEventArgs routedEventArgs)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;

        // if button wasn't the current playing playlist, switch playlists
        if (!Equals(ViewModel.CurrentPlaylistPlaying, selectedVm))
        {
            ViewModel.PlayFrom(selectedVm);
            return;
        }

        // else toggle pause and play
        _player.TogglePlaying();
    }

    private void ClearQueueClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearUserQueue();
    }

    private void GoToCurrentSongPlaylist(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentSong == null) return;
        PlaylistViewModel songPlaylist = ViewModel.GetPlaylistModel(ViewModel.CurrentSong);
        OpenPlaylist(songPlaylist);
    }

    private void GoToCurrentPlaylistPlaying(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPlaylistPlaying == null) return;
        OpenPlaylist(ViewModel.CurrentPlaylistPlaying);
    }
}

}*/