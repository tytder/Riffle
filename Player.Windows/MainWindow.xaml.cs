using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Riffle.Player.Windows.Services;
using Riffle.Core.Audio;
using Application = System.Windows.Application;

namespace Riffle.Player.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IAudioPlayer _player;

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
                Debug.Text = value ? "Y" : "N";
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
        
        public Track Track { get; set; }

        private void Player_TrackLoaded(object sender, EventArgs e)
        {
            TxtTotalTime.Text = _player.TotalTime.TotalSeconds.ToMmSs();
            SeekBar.Maximum = _player.TotalTime.TotalSeconds;
            SeekBar.Value = 0;
            TxtSongTitle.Text = _player.SongTitle;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.HasTrackLoaded) // TODO: later on just preload
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed && IsDraggingSeekBarThumb)
                {
                    SeekBar.Value = _player.CurrentTime.TotalSeconds;
                    SeekBarWasRecentlyAutoUpdated = true;
                }
            }
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            // change this path to a local .mp3 or .wav you have on disk
            var dialog = new OpenFileDialog { Filter = "Audio files|*.mp3;*.wav" };
            if ((int)dialog.ShowDialog() % 5 == 1)
                _player.Play(dialog.FileName);
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

        private void OnStop(object sender, RoutedEventArgs e)
        {
            _player.Stop();
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
                _player.Play(dialog.FileName);
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

    }
}