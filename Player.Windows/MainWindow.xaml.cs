using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Riffle.Core;
using Riffle.Player.Windows.Services;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Song> CurrentSongCollection { get; set; }
        private ObservableCollection<Playlist> PlaylistCollection { get; set; }
        
        private readonly IAudioPlayer _player;
        private readonly QueuePlayer _queuePlayer;
        
        private Song _currentSongPlaying;
        public Song CurrentSongPlaying
        {
            get => _currentSongPlaying;
            set
            {
                if (!Equals(_currentSongPlaying, value))
                {
                    _currentSongPlaying = value;
                    OnPropertyChanged(nameof(CurrentSongPlaying));
                }
            }
        }
        
        
        public Playlist CurrentPlaylistPlaying, CurrentOpenPlaylist;
        private readonly Playlist _allSongsPlaylist;
        private bool _isQueueOpen;
        private Color _buttonInactiveColor;
        
        public Track Track { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _player = new NAudioAudioPlayer();
            
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            
            _player.TrackLoaded += Player_TrackLoaded;
            
            Loaded += OnLoaded;
            
            // Setting current playlist
            CurrentSongCollection = new ObservableCollection<Song>();
            PlaylistContent.Items.Clear();
            PlaylistContent.ItemsSource = CurrentSongCollection;
            
            // Setting playlists list
            PlaylistCollection = new ObservableCollection<Playlist>();
            PlaylistList.Items.Clear();
            PlaylistList.ItemsSource = PlaylistCollection;
            _allSongsPlaylist = new Playlist("All Songs");
            PlaylistCollection.Add(_allSongsPlaylist);
            CurrentPlaylistPlaying = _allSongsPlaylist;
            PlaylistList.SelectedIndex = 0;
            
            // Setting queue list
            _queuePlayer = new QueuePlayer(_player);
            QueueListView.Items.Clear();
            
            _buttonInactiveColor = Color.FromRgb(80, 80, 80);
            BtnLoop.Background = new SolidColorBrush(_buttonInactiveColor);
            BtnShuffle.Background = new SolidColorBrush(_buttonInactiveColor);
        }

        private bool _isDraggingSeekBarThumb;
        private bool IsDraggingSeekBarThumb
        {
            get => _isDraggingSeekBarThumb;
            set
            {
                _isDraggingSeekBarThumb = value;
            }
        }

        private bool _seekBarWasRecentlyAutoUpdated;
        private bool SeekBarWasRecentlyAutoUpdated
        {
            get => _seekBarWasRecentlyAutoUpdated;
            set
            {
                _seekBarWasRecentlyAutoUpdated = value;
            }
        }

        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SeekBar.ApplyTemplate(); // ensure the template is created
            Track = (Track)SeekBar.Template.FindName("PART_Track", SeekBar);
            if (Track.Thumb != null)
            {
                Track.Thumb.PreviewMouseLeftButtonDown += (s, args) =>
                {
                    // Calculate clicked position
                    Point pos = args.GetPosition(Track);
                    double newValue = Track.ValueFromPoint(pos);

                    // Set value immediately so the Thumb is in the correct place before drag starts
                    SeekBar.Value = newValue;

                    // Now let the Thumb continue dragging normally
                    args.Handled = false;
                };
            }
        }
        


        private void Player_TrackLoaded(object sender, Song song)
        {
            TxtTotalTime.Text = song.Duration.TotalSeconds.ToMmSs();
            SeekBar.Maximum = song.Duration.TotalSeconds;
            SeekBar.Value = 0;
            TxtSongTitle.Text = song.Title;
            TxtArtistName.Text = song.Artist;
            _isDraggingSeekBarThumb = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.HasTrackLoaded) // TODO: later on just preload
            {
                if (!IsDraggingSeekBarThumb)
                {
                    SeekBar.Value = _player.CurrentTime.TotalSeconds;
                    SeekBarWasRecentlyAutoUpdated = true;
                }
            }
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

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            if (IsDraggingSeekBarThumb)
            {
                IsDraggingSeekBarThumb = false;
                if (_player.HasTrackLoaded)
                {
                    _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
                    TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
                }
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(Track.Thumb);
            Track.Thumb.RaiseEvent(
                new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent,
                    Source = e.Source,
                });
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!SeekBarWasRecentlyAutoUpdated)
            {
                IsDraggingSeekBarThumb = true;
            }
            if (_player.HasTrackLoaded)
                TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
            SeekBarWasRecentlyAutoUpdated = false;
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_player != null)
            {
                _player.Volume = (float)VolumeBar.Value;
                TxtVolumePercentage.Text = ((int)VolumeBar.Value) + "%";
            }
        }

        private void BtnImportSong_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Audio files|*.mp3;*.wav" };
            if ((int)dialog.ShowDialog() % 5 == 1)
                ShowSongMetadataDialog(dialog.FileName);
        }
        
        private void ShowSongMetadataDialog(string filePath)
        {
            SongImportData metadataWindow = new SongImportData
            {
                FilePath = filePath.Split('\\')[^1]
            };
            if (metadataWindow.ShowDialog() == true)
            {
                var song = TagLib.File.Create(filePath);
                string title = metadataWindow.SongTitle;
                string artist = metadataWindow.ArtistName;
                TimeSpan duration = song.Properties.Duration;

                Song newSong = new Song
                (
                    title,
                    artist,
                    duration,
                    filePath
                );
                
                CurrentSongCollection.Add(newSong);

                if (!Equals(CurrentPlaylistPlaying, _allSongsPlaylist))
                {
                    if (!_allSongsPlaylist.PlaylistItems.Contains(newSong))
                        _allSongsPlaylist.PlaylistItems.Add(newSong);
                }
            }
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
            if (PlaylistContent.SelectedItem is Song selectedSong)
            {
                _queuePlayer.PlayFrom(selectedSong, CurrentSongCollection);
                QueueListView.ItemsSource = _queuePlayer.QueueCollection;
                //_player.Play(selectedSong);
                
                CurrentSongPlaying = selectedSong;
                CurrentPlaylistPlaying = CurrentOpenPlaylist;
            }
        }

        private void PlaylistList_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = PlaylistContent.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            PlaylistView.Columns[0].Width = totalWidth * 4/12;
        }

        private void PlaylistList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistList.SelectedItem is Playlist selectedPlaylist)
            {
                if (Equals(selectedPlaylist, CurrentOpenPlaylist)) return;
                CurrentOpenPlaylist.PlaylistItems = new ObservableCollection<Song>(CurrentSongCollection);
                CurrentSongCollection.Clear();
                foreach (var item in selectedPlaylist.PlaylistItems)
                    CurrentSongCollection.Add(item);
                CurrentOpenPlaylist = selectedPlaylist;
                if (Equals(selectedPlaylist, CurrentPlaylistPlaying))
                {
                    if (CurrentSongPlaying == null)
                    {
                        CurrentSongPlaying = _queuePlayer.CurrentSong;
                    }
                }
                else
                {
                    CurrentSongPlaying = null;
                }
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow playlistWindow = new NewPlaylistWindow();
            if (playlistWindow.ShowDialog() == true)
            {
                var newPlaylist = new Playlist (playlistWindow.PlaylistName);
                PlaylistCollection.Add(newPlaylist);
                CurrentPlaylistPlaying.PlaylistItems = new ObservableCollection<Song>(CurrentSongCollection);
                CurrentSongCollection.Clear();
                foreach (var item in newPlaylist.PlaylistItems)
                    CurrentSongCollection.Add(item);
                CurrentPlaylistPlaying = newPlaylist;
                PlaylistList.SelectedItem = newPlaylist;
            }
        }

        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistList.SelectedItem is Playlist selectedPlaylist)
            {
                if (Equals(selectedPlaylist, _allSongsPlaylist)) return;
            }
            DeletePlaylistWindow deletePlaylistWindow = new DeletePlaylistWindow();
            if (deletePlaylistWindow.ShowDialog() == true)
            {
                CurrentSongCollection.Clear();
                PlaylistCollection.Remove(CurrentPlaylistPlaying);
                CurrentPlaylistPlaying = _allSongsPlaylist;
                PlaylistList.SelectedItem = _allSongsPlaylist;
            }
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
            _queuePlayer.Loop = !_queuePlayer.Loop;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}




/*private void SeekBar_OnIsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
{
    IsDraggingSeekBarThumb = SeekBar.IsMouseCaptureWithin;
}

private void SeekBar_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.Source is Slider slider)
    {
        // Force the thumb to move to the click position
        double clickValue = SeekBar.Minimum +
                            (SeekBar.Maximum - SeekBar.Minimum) *
                            (e.GetPosition(Track).X / slider.ActualWidth);

        SeekBar.Value = clickValue;

        // Grab the thumb manually
        if (Track.Thumb != null)
        {
            Track.Thumb.RaiseEvent(
                new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent,
                    Source = e.Source,
                });

            //Track.Thumb.CaptureMouse();
        }

        e.Handled = true; // prevent default behavior
    }
}*/
