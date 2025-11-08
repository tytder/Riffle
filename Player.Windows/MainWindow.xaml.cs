#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Riffle.Core;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;
using Riffle.Core.Services;
using Riffle.Player.Windows.Services;
using Riffle.Player.Windows.ViewModels;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Riffle.Player.Windows
{
    public partial class MainWindow
    {
        private readonly NAudioAudioPlayer _player;
        private readonly PlaybackManager _playbackManager;
        private readonly MusicService _musicService;
        private readonly Color _buttonInactiveColor;
        
        public MainWindowViewModel ViewModel { get; }
        
        private bool _isTeleportingSeekBarThumb;

        private bool _seekBarWasRecentlyAutoUpdated;
        private bool _isDraggingSeekBar;
        
        private bool _isQueueOpen;
        
        public MainWindow(MusicService musicService)
        {
            InitializeComponent();

            _musicService = musicService;
            _player = new NAudioAudioPlayer();
            ViewModel = new MainWindowViewModel(musicService);
            PlaylistList.SelectedIndex = 0;
            DataContext = ViewModel;
            _playbackManager = new PlaybackManager(_player);
            
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += Timer_Tick;
            timer.Start();

            _player.TrackLoaded += Player_TrackLoaded;
            _player.StopAllCalled += OnStopCalled;
            Loaded += OnLoaded;
            
            _buttonInactiveColor = Color.FromRgb(80, 80, 80);
            BtnLoop.Background = new SolidColorBrush(_buttonInactiveColor);
            BtnShuffle.Background = new SolidColorBrush(_buttonInactiveColor);
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _player.Dispose();
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SeekBar.ApplyTemplate(); // ensure the template is created
            
            SeekBar.AddHandler(
                UIElement.MouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SeekBar_PreviewMouseLeftButtonDown),
                handledEventsToo: true);
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
        
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        
        private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var slider = (Slider)sender;
            Point clickPoint = e.GetPosition(slider);

            // Calculate new value
            double ratio = clickPoint.X / slider.ActualWidth;
            slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;

            // Start manual drag
            _isDraggingSeekBar = true;
            Mouse.Capture(slider);
            e.Handled = true;
        }

        private void SeekBar_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSeekBar) return;
            var slider = (Slider)sender;
            Point pos = e.GetPosition(slider);
            double ratio = pos.X / slider.ActualWidth;
            slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;
            if (_player.HasTrackLoaded) TxtCurrentTime.Text = slider.Value.ToMmSs();
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingSeekBar) return;
            _isDraggingSeekBar = false;
            _isTeleportingSeekBarThumb = false;
            Mouse.Capture(null);
            _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
        }

        private void Player_TrackLoaded(object? sender, TrackLoadedEventArgs e)
        {
            TxtTotalTime.Text = e.SongLoaded.Duration.TotalSeconds.ToMmSs();
            SeekBar.Maximum = e.SongLoaded.Duration.TotalSeconds;
            SeekBar.Value = 0;
            TxtSongTitle.Text = e.SongLoaded.Title;
            TxtArtistName.Text = e.SongLoaded.Artist;
            _isTeleportingSeekBarThumb = false;
            QueueListView.ItemsSource = _playbackManager.Queue;
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
            _player.TogglePlay();
            SetPauseResume(_player.IsPlaying);
        }

        private void SetPauseResume(bool shouldPause)
        {
            var selectedPlaylistViewModel = PlaylistList.SelectedItem as PlaylistViewModel;
            if (shouldPause)
            {
                BtnPauseResume.Content = "▶";
                BtnPauseResume.Padding = new Thickness(.5, -2, .5, -0.5);
                if (Equals(ViewModel.CurrentPlaylistPlaying, selectedPlaylistViewModel))
                {
                    PlaylistPlayBtn.Content = "▶";
                    PlaylistPlayBtn.Padding = new Thickness(5,0,5,0);
                    PlaylistPlayBtn.FontSize = 12;
                }
            }
            else
            {
                BtnPauseResume.Content = "⏸";
                BtnPauseResume.Padding = new Thickness(-2, -2, -2, -0.5);
                if (Equals(ViewModel.CurrentPlaylistPlaying, selectedPlaylistViewModel))
                {
                    PlaylistPlayBtn.Content = "⏸";
                    PlaylistPlayBtn.Padding = new Thickness(-2,-2,-2,-0);
                    PlaylistPlayBtn.FontSize = 18;
                }
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_seekBarWasRecentlyAutoUpdated && !_isDraggingSeekBar)
            {
                _isTeleportingSeekBarThumb = true;
            }
            if (_player.HasTrackLoaded) TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
            _seekBarWasRecentlyAutoUpdated = false;
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            _player.SetVolume((float)VolumeBar.Value / 100);
            TxtVolumePercentage.Text = ((int)VolumeBar.Value) + "%";
        }

        private void BtnImportSong_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Audio files|*.mp3;*.wav",
                Multiselect = true
            };
            if ((int)dialog.ShowDialog() % 5 != 1) return; // check for any ok sign
            foreach (var file in dialog.FileNames)
            {
                ShowSongMetadataDialog(file);
            }
        }
        
        private void ShowSongMetadataDialog(string filePath)
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
            Playlist? playlist = selectedVm.Playlist;

            Song newSong = new Song(title, artist, duration, filePath);

            _musicService.AddSong(newSong, playlist);

            // Refresh the songs in the viewmodel
            ViewModel.SongsViewModel.RefreshSongs();
            ViewModel.SongsViewModel.LoadSongs(ViewModel.SelectedPlaylist);

            if (_player.IsPlaying) return; // if no track currently playing, switch to imported song.
            PlaySong(newSong);
        }

        private void PlaylistContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = PlaylistContent.ActualWidth 
                                - SystemParameters.VerticalScrollBarWidth 
                                - GridView.Columns[0].Width 
                                - PlaylistContent.Padding.Left 
                                - PlaylistContent.Padding.Right;

            GridView.Columns[1].Width = totalWidth * 4/12;
            GridView.Columns[2].Width = totalWidth * 2/12;
            GridView.Columns[3].Width = totalWidth * 1.5/12;
            GridView.Columns[4].Width = totalWidth * 4.65/12;
        }

        private void PlaylistContent_OnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (PlaylistContent.SelectedItem is not Song selectedSong) return;
            PlaySong(selectedSong);
        }

        private void PlaySong(Song selectedSong)
        {
            // Determine which playlist view-model is currently selected in the sidebar
            var selectedPlaylistViewModel = PlaylistList.SelectedItem as PlaylistViewModel;

            // Decide the concrete list of songs to play:
            // - if selectedVm is null or represents "All Songs" (Playlist == null) -> use the songs currently shown in the SongsViewModel
            // - otherwise ask the MusicService for the songs that belong to that playlist
            List<Song> playListSongs = selectedPlaylistViewModel?.Playlist == null ? 
                ViewModel.SongsViewModel.GetAllSongs() :
                _musicService.GetSongsForPlaylist(selectedPlaylistViewModel.Playlist);

            // Start playback
            _playbackManager.PlayFrom(selectedSong, playListSongs);

            // Update "currently playing" state in the MainWindowViewModel
            ViewModel.CurrentSongPlaying = selectedSong;
            ViewModel.CurrentPlaylistPlaying = selectedPlaylistViewModel; // null means "All Songs"
        }

        private void PlaylistList_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            PlaylistView.Columns[0].Width = totalWidth * 4/12;
        }

        private void PlaylistList_OnSelected(object sender, RoutedEventArgs e)
        {
            if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm) return;
                        ViewModel.SongsViewModel.LoadSongs(selectedVm);
                        PlaylistInfo.Text = ViewModel.SelectedPlaylistInfo;
        }
        
        private void PlaylistList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedPlaylistViewModel = PlaylistList.SelectedItem as PlaylistViewModel;
            PlayFirstSongFromPlaylist(selectedPlaylistViewModel);
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow playlistWindow = new NewPlaylistWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (playlistWindow.ShowDialog() != true) return;
            var newPlaylist = _musicService.CreatePlaylist(playlistWindow.PlaylistName);
            ViewModel.SidebarViewModel.RefreshPlaylists();
            var playlistToSelect = ViewModel.SidebarViewModel.GetPlaylist(newPlaylist.Id);
            PlaylistList.SelectedItem = playlistToSelect;
        }

        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
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
            _musicService.DeletePlaylist(selectedVm.Playlist);

            // Refresh the sidebar list and select "All Songs"
            ViewModel.SidebarViewModel.RefreshPlaylists();

            // Try to select the "All Songs" entry in the sidebar
            var allSongsVm = ViewModel.SidebarViewModel.Playlists.FirstOrDefault(p => p.Playlist == null);
            if (allSongsVm != null)
                ViewModel.SelectedPlaylist = allSongsVm;
        }

        private void Queue_OnClick(object sender, RoutedEventArgs e)
        {
            _isQueueOpen = !_isQueueOpen;

            double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            
            QueueOverlay.Visibility = _isQueueOpen ?  Visibility.Visible : Visibility.Collapsed;
            QueueOverlayColumn.Width = _isQueueOpen ? totalWidth * 5/12 : 0;
        }

        private void BtnLoop_OnClick(object sender, RoutedEventArgs e)
        {
            _playbackManager.ToggleLoop();
            BtnLoop.Background = new SolidColorBrush(_playbackManager.IsLooping ? Colors.White : _buttonInactiveColor);
        }

        private void BtnNextSong_OnClick(object sender, RoutedEventArgs e)
        {
            _playbackManager.SkipToNextSong();
        }

        private void BtnPreviousSong_OnClick(object sender, RoutedEventArgs e)
        {
            _playbackManager.SkipToPrevSong();
        }

        private void OnStopCalled(object? sender, EventArgs eventArgs)
        {
            TxtSongTitle.Text = _player.SongTitle;
            TxtCurrentTime.Text =  _player.CurrentTime.TotalSeconds.ToMmSs();
            TxtTotalTime.Text  = _player.TotalTime.TotalSeconds.ToMmSs();
            TxtArtistName.Text = "";
            SeekBar.Value = 0;
        }

        private void PlaylistPlayBtn_OnBtnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var selectedPlaylistViewModel = PlaylistList.SelectedItem as PlaylistViewModel;
            
            // if button wasn't the current playing playlist, switch playlists
            if (!Equals(ViewModel.CurrentPlaylistPlaying, selectedPlaylistViewModel))
            {
                PlayFirstSongFromPlaylist(selectedPlaylistViewModel);
                return;
            }
            
            // else toggle pause and play
            _player.TogglePlay();
            SetPauseResume(_player.IsPlaying);
        }

        private void PlayFirstSongFromPlaylist(PlaylistViewModel? selectedPlaylistViewModel)
        {
            // Decide the concrete list of songs to play:
            // - if selectedVm is null or represents "All Songs" (Playlist == null) -> use the songs currently shown in the SongsViewModel
            // - otherwise ask the MusicService for the songs that belong to that playlist
            List<Song> playListSongs = selectedPlaylistViewModel?.Playlist == null ? 
                ViewModel.SongsViewModel.GetAllSongs() :
                _musicService.GetSongsForPlaylist(selectedPlaylistViewModel.Playlist);

            var firstSongInPlaylist = selectedPlaylistViewModel?.GetFirstSong() ?? playListSongs[0];
            
            // Start playback
            _playbackManager.PlayFrom(firstSongInPlaylist, playListSongs);
            
            // Update "currently playing" state in the MainWindowViewModel
            ViewModel.CurrentSongPlaying = firstSongInPlaylist;
            ViewModel.CurrentPlaylistPlaying = selectedPlaylistViewModel;
            
            SetPauseResume(false);
        }
    }
}


