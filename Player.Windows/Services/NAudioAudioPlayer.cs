#nullable enable
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
                
                if (!value) _outputDevice.Pause();
                else _outputDevice.Play();
                
                var handler = PlayingStateChanged;
                handler?.Invoke(this, new PlayingStateEventArgs(value));
            }
        }

        public bool HasTrackLoaded { get; private set; }
        public string SongTitle { get; private set; } = "No file selected";

        public event EventHandler<TrackEventArgs>? TrackLoaded;
        public event EventHandler? TrackEnded;
        public event EventHandler? StopAllCalled;
        public event EventHandler<PlayingStateEventArgs>? PlayingStateChanged; 
        
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
            _mixer.MixerInputEnded += OnPlaybackEnded;
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
            _outputDevice.Play();
            HasTrackLoaded = true;
            var handler = TrackLoaded;
            handler?.Invoke(this, new TrackEventArgs(song));
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
                        // Other channel counts will need a more general converter.
                        throw new InvalidOperationException("Unsupported channel conversion");
                    }
                }
            }

            return input;
        }

        public void SetVolume(float volume)
        {
            // TODO: should we allow more than 100%?
            _volume = Math.Clamp(volume, 0, 1);
            _outputDevice.Volume = _volume;
        }

        private void OnPlaybackEnded(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var handler = TrackEnded;
                handler?.Invoke(this, e);
            });
        }

        public void TogglePlaying()
        {
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
            SongTitle = "No file selected";
            HasTrackLoaded = false;
            IsPlaying = false;
            _outputDevice.Stop();
            var handler = StopAllCalled;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            StopAll();
            _outputDevice.Stop();
            _outputDevice.Dispose();
            _mixer.MixerInputEnded -= OnPlaybackEnded;
        }
    }
}