#nullable enable
using System;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;

namespace Player.Desktop.Models
{
    public class VlcAudioPlayer : IAudioPlayer
    {
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;

                if (value) _mediaPlayer.Play();
                else _mediaPlayer.Pause();
                
                Dispatcher.UIThread.Post(
                    () => PlayingStateChanged?.Invoke(this, new PlayingStateEventArgs(value)));
            }
        }

        public bool HasTrackLoaded { get; private set; }
        public string SongTitle { get; private set; } = "No file selected";

        public event EventHandler<TrackEventArgs>? TrackLoaded;
        public event EventHandler? TrackEnded;
        public event EventHandler? StopAllCalled;
        public event EventHandler<PlayingStateEventArgs>? PlayingStateChanged;

        public TimeSpan CurrentTime =>
            TimeSpan.FromMilliseconds(_mediaPlayer.Time);

        public TimeSpan TotalTime =>
            TimeSpan.FromMilliseconds(_mediaPlayer.Length);

        private float _volume = 1f;
        public float Volume => Math.Clamp(_volume, 0, 1);

        public VlcAudioPlayer()
        {
            _libVlc = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVlc);

            _mediaPlayer.EndReached += MediaPlayerOnEndReached;
        }

        private void MediaPlayerOnEndReached(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() => TrackEnded?.Invoke(sender, e));
        }

        public void Play(Song song)
        {
            StopAll();

            var media = new Media(_libVlc, song.FilePath, FromType.FromPath);

            _mediaPlayer.Media = media;
            _mediaPlayer.Play();

            SongTitle = song.Title;
            HasTrackLoaded = true;
            IsPlaying = true;

            Dispatcher.UIThread.Post(() => TrackLoaded?.Invoke(this, new TrackEventArgs(song)));
        }

        public void SetVolume(float volume)
        {
            _volume = Math.Clamp(volume, 0, 1);
            _mediaPlayer.Volume = (int)(_volume * 100); // VLC uses 0–100
        }

        public void TogglePlaying()
        {
            IsPlaying = !IsPlaying;
        }

        public void Seek(TimeSpan time)
        {
            if (HasTrackLoaded)
            {
                _mediaPlayer.Time = (long)time.TotalMilliseconds;
            }
        }

        public void StopAll()
        {
            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Stop();

            _mediaPlayer.Media?.Dispose();
            //_mediaPlayer.Media = null;

            SongTitle = "No file selected";
            HasTrackLoaded = false;
            IsPlaying = false;

            Dispatcher.UIThread.Post(() => StopAllCalled?.Invoke(this, EventArgs.Empty));
        }

        public void Dispose()
        {
            StopAll();

            _mediaPlayer.Dispose();
            _libVlc.Dispose();
        }
    }
}