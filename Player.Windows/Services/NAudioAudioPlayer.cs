using System;
using Riffle.Core.Audio;
using NAudio.Wave;

namespace Riffle.Player.Windows.Services
{
    public class NAudioAudioPlayer : IAudioPlayer
    {
        public bool IsPlaying { get; set; }
        public bool HasTrackLoaded { get; set; }
        public event EventHandler? TrackLoaded;

        public TimeSpan CurrentTime => _audioFile?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan TotalTime => _audioFile?.TotalTime ?? TimeSpan.Zero;

        private IWavePlayer? _outputDevice;
        private AudioFileReader? _audioFile;

        public void Play(string filePath)
        {
            Stop(); // reset if something is already playing

            _outputDevice = new WaveOutEvent();
            _audioFile = new AudioFileReader(filePath);
            _outputDevice.Init(_audioFile);
            _outputDevice.Play();
            IsPlaying = true;
            HasTrackLoaded = true;
            TrackLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            _outputDevice?.Pause();
            IsPlaying = false;
        }

        public void Resume()
        {
            if (IsPlaying) return;
            _outputDevice?.Play();
            IsPlaying = true;
        }
        
        public void Seek(TimeSpan fromSeconds)
        {
            if (HasTrackLoaded)
                _audioFile.CurrentTime = fromSeconds;
        }

        public void Stop()
        {
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _outputDevice = null;

            _audioFile?.Dispose();
            _audioFile = null;
            HasTrackLoaded = false;
        }
    }
}