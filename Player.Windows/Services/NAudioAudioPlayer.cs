using System;
using System.Collections.Generic;
using System.Windows;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Riffle.Core.Interfaces;
using Riffle.Core.Models;

namespace Riffle.Player.Windows.Services
{
    #nullable enable
    public class NAudioAudioPlayer : IAudioPlayer, IDisposable
    {
        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;
                PlayingStateChanged.Invoke(this, new PlayingStateEventArgs(value));
            }
        }

        public bool HasTrackLoaded { get; private set; }
        public string SongTitle { get; private set; } = "No File Selected";

        public event EventHandler<TrackLoadedEventArgs> TrackLoaded;
        public event EventHandler TrackEnded;
        public event EventHandler StopAllCalled;
        public event EventHandler<PlayingStateEventArgs> PlayingStateChanged; 
        
        public TimeSpan CurrentTime => FirstReader?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan TotalTime => FirstReader?.TotalTime ?? TimeSpan.Zero;

        private AudioFileReader? FirstReader => _activeInputs.Count > 0 ? _activeInputs[0].reader : null;
        
        private float _volume = 1;
        /// <summary>
        /// Takes in and returns a value between 0-1.
        /// </summary>
        public float Volume => Math.Clamp(_volume, 0, 1);

        private readonly WaveOutEvent _outputDevice;
        private readonly MixingSampleProvider _mixer;
        private readonly List<(AudioFileReader reader, ISampleProvider provider)> _activeInputs = new();

        public NAudioAudioPlayer()
        {
            _outputDevice = new WaveOutEvent();
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };
            _outputDevice.Init(_mixer);
            _outputDevice.Play();
            _mixer.MixerInputEnded += OnPlaybackStopped;
        }

        public void Play(Song song)
        {
            StopAll(); // reset if something is already playing
            var reader = new AudioFileReader(song.FilePath); // gives floating samples when ToSampleProvider called
            var input = GetValidSampleInput(reader);
            _mixer.AddMixerInput(input);
            _activeInputs.Add((reader, input));
            SongTitle = song.Title;
            IsPlaying = true;
            HasTrackLoaded = true;
            TrackLoaded.Invoke(this, new TrackLoadedEventArgs(song));
        }

        private ISampleProvider GetValidSampleInput(AudioFileReader reader)
        {
            ISampleProvider input = reader.ToSampleProvider();

            // if the input format doesn't match the mixer format, convert it:
            if (!input.WaveFormat.Equals(_mixer.WaveFormat))
            {
                // resample sample rate if needed
                if (input.WaveFormat.SampleRate != _mixer.WaveFormat.SampleRate)
                {
                    input = new WdlResamplingSampleProvider(input, _mixer.WaveFormat.SampleRate);
                }

                // convert channels if needed (mono<->stereo examples)
                if (input.WaveFormat.Channels != _mixer.WaveFormat.Channels)
                {
                    if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
                    {
                        input = new MonoToStereoSampleProvider(input);
                    }
                    else if (input.WaveFormat.Channels == 2 && _mixer.WaveFormat.Channels == 1)
                    {
                        input = new StereoToMonoSampleProvider(input);
                    }
                    else
                    {
                        // For other channel counts, you'll need a more general converter.
                        throw new InvalidOperationException("Unsupported channel conversion");
                    }
                }
            }

            return input;
        }

        public void SetVolume(float volume)
        {
            // TODO: how to get volume higher than 100%
            _volume = Math.Clamp(volume, 0, 1);
            _outputDevice.Volume = _volume;
        }

        private void OnPlaybackStopped(object? sender, SampleProviderEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() => TrackEnded.Invoke(this, EventArgs.Empty));
        }

        public void TogglePlay()
        {
            if (IsPlaying) _outputDevice.Pause();
            else _outputDevice.Play();
            
            IsPlaying = !IsPlaying;
        }
        
        public void Seek(TimeSpan fromSeconds)
        {
            if (HasTrackLoaded && FirstReader != null)
            {
                FirstReader.CurrentTime = fromSeconds;
            }
        }
        
        public void StopAll()
        {
            for (int i = _activeInputs.Count - 1; i >= 0; i--)
            {
                var (reader, provider) = _activeInputs[i];
                _mixer.RemoveMixerInput(provider);
                reader.Dispose();
                _activeInputs.RemoveAt(i);
            }
            
            _activeInputs.Clear();
            SongTitle = "No File Selected";
            HasTrackLoaded = false;
            IsPlaying = false;
            StopAllCalled.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            StopAll();
            _outputDevice.Stop();
            _outputDevice.Dispose();
            _mixer.MixerInputEnded -= OnPlaybackStopped;
        }
    }
}