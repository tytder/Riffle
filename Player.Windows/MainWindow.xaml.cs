using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Riffle.Core;
using Riffle.Player.Windows.Services;
using Riffle.Core.Audio;
using ListView = System.Windows.Controls.ListView;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Riffle.Player.Windows
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
                    OnPropertyChanged();
                }
            }
        }

        
        private Playlist _currentPlaylistPlaying;
        public Playlist CurrentPlaylistPlaying
        {
            get => _currentPlaylistPlaying;
            set
            {
                if (!Equals(_currentPlaylistPlaying, value))
                {
                    _currentPlaylistPlaying = value;
                    OnPropertyChanged();
                }
            }
        }
        private Playlist _currentPlaylistOpen;
        public Playlist CurrentPlaylistOpen
        {
            get => _currentPlaylistOpen;
            set
            {
                if (!Equals(_currentPlaylistOpen, value))
                {
                    _currentPlaylistOpen = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _isTeleportingSeekBarThumb;
        private bool IsTeleportingSeekBarThumb
        {
            get => _isTeleportingSeekBarThumb;
            set
            {
                _isTeleportingSeekBarThumb = value;
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
            
            // Setting playlists list
            PlaylistCollection = new ObservableCollection<Playlist>();
            PlaylistList.Items.Clear();
            PlaylistList.ItemsSource = PlaylistCollection;
            _allSongsPlaylist = new Playlist("All Songs");
            PlaylistCollection.Add(_allSongsPlaylist);
            PlaylistList.SelectedIndex = 0;
            
            // Setting current playlist
            CurrentPlaylistOpen = _allSongsPlaylist;
            PlaylistContent.Items.Clear();
            PlaylistContent.ItemsSource = CurrentPlaylistOpen.PlaylistItems;
            
            // Setting queue list
            _queuePlayer = new QueuePlayer(_player);
            QueueListView.Items.Clear();
            
            _buttonInactiveColor = Color.FromRgb(80, 80, 80);
            BtnLoop.Background = new SolidColorBrush(_buttonInactiveColor);
            BtnShuffle.Background = new SolidColorBrush(_buttonInactiveColor);
        }
        
        private static readonly Stopwatch _logSw = Stopwatch.StartNew();
        private void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"[{_logSw.ElapsedMilliseconds:0000}ms][T{Thread.CurrentThread.ManagedThreadId}] {msg}");
        }

        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SeekBar.ApplyTemplate(); // ensure the template is created
            Track = (Track)SeekBar.Template.FindName("PART_Track", SeekBar);
            
            if (Track?.Thumb != null)
            {
                //Track.Thumb.DragCompleted += ThumbOnDragCompleted;
                //Track.Thumb.DragDelta += ThumbOnDragDelta;
                
                /*
                var t = Track.Thumb;
                t.PreviewMouseLeftButtonDown += (s, e) => Log($"Thumb.PreviewMouseLeftButtonDown   Handled={e.Handled} IsDragging={t.IsDragging}");
                t.MouseLeftButtonDown         += (s, e) => Log($"Thumb.MouseLeftButtonDown         Handled={e.Handled} IsDragging={t.IsDragging}");
                t.DragStarted                 += (s, e) => Log("Thumb.DragStarted");
                t.DragDelta                   += (s, e) => Log($"Thumb.DragDelta dx={e.HorizontalChange:0.##} dy={e.VerticalChange:0.##}");
                t.DragCompleted               += (s, e) => Log($"Thumb.DragCompleted -> SeekBar.Value={SeekBar.Value}");
                t.GotMouseCapture             += (s, e) => Log($"Thumb.GotMouseCapture   Mouse.Captured={Mouse.Captured?.GetType().Name ?? "null"}");
                t.LostMouseCapture            += (s, e) => Log($"Thumb.LostMouseCapture  Mouse.Captured={Mouse.Captured?.GetType().Name ?? "null"}");
                */

            }

            // Handle clicks on the track (not just the thumb)
            //SeekBar.PreviewMouseLeftButtonDown += SeekBar_PreviewMouseLeftButtonDown;
            SeekBar.AddHandler(
                UIElement.MouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SeekBar_PreviewMouseLeftButtonDown),
                handledEventsToo: true);
            /*SeekBar.PreviewMouseLeftButtonDown += (s, e) => Log($"Slider.PreviewMouseLeftButtonDown  OrigSource={e.OriginalSource.GetType().Name} Handled={e.Handled}");
            //SeekBar.PreviewMouseMove           += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) Log("Slider.PreviewMouseMove (LMB down)"); };
            this.MouseLeftButtonUp             += (s, e) => Log($"Window.MouseLeftButtonUp  Mouse.Captured={Mouse.Captured?.GetType().Name ?? "null"}");

            InputManager.Current.PreProcessInput += (s, e) =>
            {
                var input = e.StagingItem.Input;
                if (input is MouseButtonEventArgs mbe)
                    Log($"RAW PreProcessInput: MouseButton {mbe.ChangedButton} State? {mbe.ButtonState} Handled={mbe.Handled} Timestamp={mbe.Timestamp}");
                /*else if (input is MouseEventArgs me)
                    Log($"RAW PreProcessInput: MouseMove  LeftButton={me.LeftButton}");#1#
            };*/
            
            /*Track.Thumb.DragCompleted += ThumbOnDragCompleted;
            Track.Thumb.DragDelta += ThumbOnDragDelta;
            if (Track.Thumb != null)
            {
                Track.PreviewMouseLeftButtonDown += (s, args) =>
                {
                    // Calculate clicked position
                    Point pos = args.GetPosition(Track);
                    double newValue = Track.ValueFromPoint(pos);

                    // Set value immediately so the Thumb is in the correct place before drag starts
                    SeekBar.Value = newValue;

                    Track.Thumb.CaptureMouse();
                    Track.Thumb.RaiseEvent(
                        new MouseButtonEventArgs(args.MouseDevice, args.Timestamp, MouseButton.Left)
                        {
                            RoutedEvent = MouseLeftButtonDownEvent,
                            Source = args.Source,
                        });
                    
                    args.Handled = false;
                };
            }*/
        }

        private bool _isDraggingSeekBar;
        
        private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*if (Track == null || Track.Thumb == null)
                return;

            // Move Thumb to the Mouse location

            Point pt = e.MouseDevice.GetPosition(Track);

            double newValue = Track.ValueFromPoint(pt);
            SeekBar.Value = newValue;
            
            // Simulate drag start: forward the MouseDown to the thumb
            Track.Thumb.RaiseEvent(
                new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent,
                    Source = e.Source
                });

            // Ensure thumb has mouse capture
            //Track.Thumb.CaptureMouse();

            e.Handled = true;*/
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
            if (_player.HasTrackLoaded)
                TxtCurrentTime.Text = slider.Value.ToMmSs();
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingSeekBar) return;
            _isDraggingSeekBar = false;
            _isTeleportingSeekBarThumb = false;
            Mouse.Capture(null);
            _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
        }


        private void ThumbOnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_player.HasTrackLoaded)
                TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
        }

        private void ThumbOnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
        }


        private void Player_TrackLoaded(object sender, Song song)
        {
            TxtTotalTime.Text = song.Duration.TotalSeconds.ToMmSs();
            SeekBar.Maximum = song.Duration.TotalSeconds;
            SeekBar.Value = 0;
            TxtSongTitle.Text = song.Title;
            TxtArtistName.Text = song.Artist;
            IsTeleportingSeekBarThumb = false;
            QueueListView.ItemsSource = _queuePlayer.Queue;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.HasTrackLoaded) // TODO: later on just preload
            {
                if (!_isDraggingSeekBar && !IsTeleportingSeekBarThumb)
                {
                    SeekBarWasRecentlyAutoUpdated = true;
                    SeekBar.Value = _player.CurrentTime.TotalSeconds;
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
            /*Mouse.Capture(null);
            if (IsDraggingSeekBarThumb)
            {
                IsDraggingSeekBarThumb = false;
                if (_player.HasTrackLoaded)
                {
                    _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
                    TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
                }
            }*/
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*Mouse.Capture(Track.Thumb);
            Track.Thumb.RaiseEvent(
                new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent,
                    Source = e.Source,
                });*/
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!SeekBarWasRecentlyAutoUpdated && !_isDraggingSeekBar)
            {
                IsTeleportingSeekBarThumb = true;
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
            var dialog = new OpenFileDialog
            {
                Filter = "Audio files|*.mp3;*.wav",
                Multiselect = true
            };
            if ((int)dialog.ShowDialog() % 5 == 1)
            {
                foreach (var file in dialog.FileNames)
                {
                    ShowSongMetadataDialog(file);
                }
            }
        }
        
        private void ShowSongMetadataDialog(string filePath)
        {
            var tagFile = TagLib.File.Create(filePath);
            
            string suggestedTitle = !string.IsNullOrEmpty(tagFile.Tag.Title)
                ? tagFile.Tag.Title
                : System.IO.Path.GetFileNameWithoutExtension(filePath);
            
            string suggestedArtist = (tagFile.Tag.Performers != null && tagFile.Tag.Performers.Length > 0)
                ? tagFile.Tag.Performers[0]
                : string.Empty;
            
            SongImportData metadataWindow = new SongImportData
            {
                FilePath = System.IO.Path.GetFileName(filePath),
            };
            
            metadataWindow.TxtSongTitle.Text = suggestedTitle;
            metadataWindow.TxtArtistName.Text = suggestedArtist;
            
            if (metadataWindow.ShowDialog() == true)
            {
                string title = metadataWindow.SongTitle;
                string artist = metadataWindow.ArtistName;
                TimeSpan duration = tagFile.Properties.Duration;

                Song newSong = new Song
                (
                    title,
                    artist,
                    duration,
                    filePath
                );
                
                if (PlaylistList.SelectedItem is Playlist currentOpenPlaylist)
                {
                    currentOpenPlaylist.PlaylistItems.Add(newSong);

                    if (!Equals(currentOpenPlaylist, _allSongsPlaylist))
                    {
                        if (!_allSongsPlaylist.PlaylistItems.Contains(newSong))
                            _allSongsPlaylist.PlaylistItems.Add(newSong);
                    }
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
                _queuePlayer.PlayFrom(selectedSong, CurrentPlaylistOpen.PlaylistItems.ToList());
                
                CurrentSongPlaying = selectedSong;
                if (PlaylistList.SelectedItem is Playlist currentOpenPlaylist)
                {
                    CurrentPlaylistPlaying = currentOpenPlaylist;
                }
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
                PlaylistContent.ItemsSource = selectedPlaylist.PlaylistItems;
                CurrentPlaylistOpen = selectedPlaylist;
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow playlistWindow = new NewPlaylistWindow();
            if (playlistWindow.ShowDialog() == true)
            {
                var newPlaylist = new Playlist (playlistWindow.PlaylistName);
                PlaylistCollection.Add(newPlaylist);
                CurrentPlaylistOpen = newPlaylist;
                PlaylistList.SelectedItem = newPlaylist;
            }
        }

        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistList.SelectedItem is Playlist selectedPlaylist)
            {
                if (Equals(selectedPlaylist, _allSongsPlaylist)) return;
                DeletePlaylistWindow deletePlaylistWindow = new DeletePlaylistWindow();
                if (deletePlaylistWindow.ShowDialog() == true)
                {
                    PlaylistCollection.Remove(selectedPlaylist);
                    PlaylistList.SelectedItem = _allSongsPlaylist;
                }
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
