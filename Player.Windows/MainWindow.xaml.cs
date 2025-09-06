using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Riffle.Player.Windows.Services;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IAudioPlayer _player;
        private bool _isDragging, _isClicking;

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
            
            MainWindow_Loaded();
        }

        private void MainWindow_Loaded()
        {
            SeekBar.ApplyTemplate();
            var track = (Track)SeekBar.Template.FindName("PART_Track", SeekBar);
            if (track != null)
            {
                track.MouseLeftButtonDown += SeekBar_OnPreviewMouseLeftButtonDown;
            }
        }


        private void Player_TrackLoaded(object sender, EventArgs e)
        {
            SeekBar.Maximum = _player.TotalTime.TotalSeconds;
            SeekBar.Value = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.HasTrackLoaded) // TODO: later on just preload
            {
                if (!_isDragging && !_isClicking)
                {
                    SeekBar.Value = _player.CurrentTime.TotalSeconds;
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
                BtnPauseResume.Content = "Resume";
            }
            else
            {
                _player.Resume();
                BtnPauseResume.Content = "Pause";
            }
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            _player.Stop();
        }

        private void SeekBar_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isClicking = false;
            if (_player.HasTrackLoaded)
            {
                _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
            }
        }

        private void SeekBar_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isClicking = true;
        }

        private void SeekBar_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SeekBar_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
        }
    }
}