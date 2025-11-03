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
using Riffle.Player.Windows.Services;
using Riffle.Core.Audio;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Riffle.Player.Windows
{
    public partial class MainWindow
    {
        private readonly NAudioAudioPlayer _player;
        private readonly MusicService _musicService;
        private readonly QueuePlayer _queuePlayer;
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
            _queuePlayer = new QueuePlayer(_player);
            DataContext = ViewModel;
            
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += Timer_Tick;
            timer.Start();

            _player.TrackLoaded += Player_TrackLoaded;
            _player.StopAllCalled += OnStopCalled;
            Loaded += OnLoaded;
            
            _buttonInactiveColor = Color.FromRgb(80, 80, 80);
            BtnLoop.Background = new SolidColorBrush(_buttonInactiveColor);
            BtnShuffle.Background = new SolidColorBrush(_buttonInactiveColor);
            
            /*InitializeComponent();
            _player = new NAudioAudioPlayer();
            _musicService = musicService;
            
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            
            _player.TrackLoaded += Player_TrackLoaded;
            
            Loaded += OnLoaded;
            
            // Setting playlists list
            PlaylistCollection = new ObservableCollection<Playlist>(_musicService.GetAllPlaylists());

            PlaylistList.Items.Clear();
            PlaylistList.ItemsSource = PlaylistCollection;
            PlaylistList.SelectedItem = null;

            // Set current playlist
            CurrentPlaylistOpen = null;
            PlaylistContent.Items.Clear();
            PlaylistContent.ItemsSource = CurrentPlaylistOpen?.PlaylistItems ?? new ObservableCollection<Song>(_musicService.GetAllSongs());*/
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _player.Dispose();
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SeekBar.ApplyTemplate(); // ensure the template is created
            //Track = (Track)SeekBar.Template.FindName("PART_Track", SeekBar);
            
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

        private void Player_TrackLoaded(Song song)
        {
            TxtTotalTime.Text = song.Duration.TotalSeconds.ToMmSs();
            SeekBar.Maximum = song.Duration.TotalSeconds;
            SeekBar.Value = 0;
            TxtSongTitle.Text = song.Title;
            TxtArtistName.Text = song.Artist;
            _isTeleportingSeekBarThumb = false;
            QueueListView.ItemsSource = _queuePlayer.Queue;
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
            if (_player.IsPlaying)
            {
                _player.Pause();
                BtnPauseResume.Content = "R";
            }
            else
            {
                _player.Resume();
                BtnPauseResume.Content = "P";
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
            _player.Volume = (float)VolumeBar.Value / 100;
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
            double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            GridView.Columns[0].Width = totalWidth * 4/12;
            GridView.Columns[1].Width = totalWidth * 2/12;
            GridView.Columns[2].Width = totalWidth * 1.5f/12;
            GridView.Columns[3].Width = totalWidth * 5/12;
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
            _queuePlayer.PlayFrom(selectedSong, playListSongs);

            // Update "currently playing" state in the MainWindowViewModel
            ViewModel.CurrentSongPlaying = selectedSong;
            ViewModel.CurrentPlaylistPlaying = selectedPlaylistViewModel; // null means "All Songs"
            
            /*if (PlaylistContent.SelectedItem is Song selectedSong)
            {
                _queuePlayer.PlayFrom(selectedSong, CurrentPlaylistOpen.PlaylistItems.ToList());
                
                CurrentSongPlaying = selectedSong;
                if (PlaylistList.SelectedItem is Playlist currentOpenPlaylist)
                {
                    CurrentPlaylistPlaying = currentOpenPlaylist;
                }
            }*/
        }

        private void PlaylistList_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            PlaylistView.Columns[0].Width = totalWidth * 4/12;
        }

        private void PlaylistList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistList.SelectedItem is PlaylistViewModel selectedVm)
                ViewModel.SongsViewModel.LoadSongs(selectedVm);
            
            /*if (PlaylistList.SelectedItem is Playlist selectedPlaylist)
            {
                PlaylistContent.ItemsSource = selectedPlaylist.PlaylistItems;
                CurrentPlaylistOpen = selectedPlaylist;
            }*/
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow playlistWindow = new NewPlaylistWindow();
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

            var deletePlaylistWindow = new DeletePlaylistWindow();
            if (deletePlaylistWindow.ShowDialog() != true) return;

            // Delete from database via service (implement if missing)
            _musicService.DeletePlaylist(selectedVm.Playlist);

            // Refresh the sidebar list and select "All Songs"
            ViewModel.SidebarViewModel.RefreshPlaylists();

            // Try to select the "All Songs" entry in the sidebar
            var allSongsVm = ViewModel.SidebarViewModel.Playlists.FirstOrDefault(p => p.Playlist == null);
            if (allSongsVm != null)
                ViewModel.SelectedPlaylist = allSongsVm;
            
            /*if (PlaylistList.SelectedItem is Playlist selectedPlaylist)
            {
                if (Equals(selectedPlaylist, _allSongsPlaylist)) return;
                DeletePlaylistWindow deletePlaylistWindow = new DeletePlaylistWindow();
                if (deletePlaylistWindow.ShowDialog() == true)
                {
                    PlaylistCollection.Remove(selectedPlaylist);
                    PlaylistList.SelectedItem = _allSongsPlaylist;
                }
            }*/
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
            _queuePlayer.ToggleLoop();
            BtnLoop.Background = new SolidColorBrush(_queuePlayer.Loop ? Colors.White : _buttonInactiveColor);
        }

        private void BtnNextSong_OnClick(object sender, RoutedEventArgs e)
        {
            _queuePlayer.SkipToNextSong();
        }

        private void BtnPreviousSong_OnClick(object sender, RoutedEventArgs e)
        {
            _queuePlayer.SkipToPrevSong();
        }

        private void OnStopCalled()
        {
            TxtSongTitle.Text = _player.SongTitle;
            TxtCurrentTime.Text =  _player.CurrentTime.TotalSeconds.ToMmSs();
            TxtTotalTime.Text  = _player.TotalTime.TotalSeconds.ToMmSs();
            TxtArtistName.Text = "";
            SeekBar.Value = 0;
        }
    }
}


