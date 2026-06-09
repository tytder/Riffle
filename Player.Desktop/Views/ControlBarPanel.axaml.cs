using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.ViewModels;
using Riffle.Core.CustomEventArgs;
using Riffle.Core.Interfaces;
using Riffle.Core.Utilities;

namespace Player.Desktop.Views;

public partial class ControlBarPanel : UserControl
{
    private IAudioPlayer _player = null!;

    private readonly Dictionary<double, double[]> _songColumnPresets = new()
    {
        { 143, new[] { 1d, 0d, 0d } },
        { 330, new[] { 0.6, 0d, 0.4 } },
        { 500, new[] { 0.50, 0.30, 0.2 } },
    };
    private MainWindowViewModel _viewModel;

    public MainWindowViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            ViewModel.SidebarViewModel.RefreshPlaylists();
            if (_player != null)
            {
                _player.PlayingStateChanged -= PlayerOnPlayingStateChanged;
                _player.TrackLoaded -= Player_TrackLoaded;
                _player.StopAllCalled -= OnStopCalled;
            }
            _player = ViewModel.Player;
            _player.PlayingStateChanged += PlayerOnPlayingStateChanged;
            _player.TrackLoaded += Player_TrackLoaded;
            _player.StopAllCalled += OnStopCalled;
        }
    }

    private bool _isDraggingBar;
    private bool _seekBarWasRecentlyAutoUpdated;
    private readonly DispatcherTimer _timer;
    private readonly IBrush _buttonInactiveBrush;
    private readonly IBrush _buttonActiveBrush;
    
    public ControlBarPanel()
    {
        InitializeComponent();
        
        DataContextChanged += OnDataContextChanged;
        
        if (DataContext is MainWindowViewModel dc) ViewModel = dc;
        else if (TopLevel.GetTopLevel(this) is Window { DataContext: MainWindowViewModel tl })
        {
            ViewModel = tl;
        }
        else
        {
            ViewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        }
        _viewModel = ViewModel;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    
        _buttonInactiveBrush = new SolidColorBrush(Colors.White);
        if (TryGetResource("ButtonInactive", ActualThemeVariant, out var inactive))
            if (inactive is SolidColorBrush inactiveBrush) _buttonInactiveBrush = inactiveBrush;
        
        _buttonActiveBrush = new SolidColorBrush(Colors.DimGray);
        if (TryGetResource("ButtonInactive", ActualThemeVariant, out var active))
            if (active is SolidColorBrush activeBrush) _buttonActiveBrush = activeBrush;
            
        BtnLoop.Background = _buttonInactiveBrush;
        BtnShuffle.Background = _buttonInactiveBrush;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        ViewModel = vm;
    }

    private void PlayerOnPlayingStateChanged(object? sender, PlayingStateEventArgs e)
    {
        SetPauseResume(e.IsPlaying);
    }

    public void Close()
    {
        if (_player != null)
        {
            _player.PlayingStateChanged -= PlayerOnPlayingStateChanged;
            _player.TrackLoaded -= Player_TrackLoaded;
            _player.StopAllCalled -= OnStopCalled;
            _player.Dispose();
        }

        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
    
    private void SeekBar_PointerReleased(object? sender, PointerCaptureLostEventArgs pointerCaptureLostEventArgs)
    {
        /*if (!_isDraggingSeekBar)
            return;

        _isDraggingSeekBar = false;
        e.Pointer.Capture(null);
        */

        _isDraggingBar = false;
        _player.Seek(TimeSpan.FromSeconds(SeekBar.Value));
    }

    private void Player_TrackLoaded(object? sender, TrackEventArgs e)
    {
        TxtTotalTime.Text = e.Song.Duration.TotalSeconds.ToMmSs();
        SeekBar.Maximum = e.Song.Duration.TotalSeconds;
        SeekBar.Value = 0;
        TxtSongTitle.Text = e.Song.Title;
        TxtArtistName.Text = e.Song.Artist;
        _isDraggingBar = false;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_player.HasTrackLoaded)
            return;
        if (_isDraggingBar)
            return;
        
        _seekBarWasRecentlyAutoUpdated = true;
        SeekBar.Value = _player.CurrentTime.TotalSeconds;
        TxtCurrentTime.Text = SeekBar.Value.ToMmSs();
    }

    private void OnPauseResume(object? sender, RoutedEventArgs e)
    {
        if (!_player.HasTrackLoaded)
            return;

        _player.TogglePlaying();
    }

    private void SetPauseResume(bool isPlaying)
    {
        SetControlBarPlaying(isPlaying);
        ViewModel.SetPlaylistHeaderPlaying(isPlaying);
    }

    private void SetControlBarPlaying(bool isPlaying)
    {
        if (!isPlaying)
        {
            BtnPauseResume.Content = "▶";
            BtnPauseResume.Padding = new Thickness(-2, -3.5, -2, -0.5);
            BtnPauseResume.FontSize = 16;
        }
        else
        {
            BtnPauseResume.Content = "⏸";
            BtnPauseResume.Padding = new Thickness(-2.5, -3.3, -2, -0.5);
            BtnPauseResume.FontSize = 20.2;
        }
    }
    
    private void SeekBar_ValueChanged(object? sender, RoutedEventArgs e)
    {
        if (!_seekBarWasRecentlyAutoUpdated)
        {
            _isDraggingBar = true;
        }

        //double totalSeconds;
        if (_player.HasTrackLoaded)
            TxtCurrentTime.Text = SeekBar.Value.ToMmSs();

        _seekBarWasRecentlyAutoUpdated = false;
    }

    private void VolumeBar_ValueChanged(object? sender, RoutedEventArgs e)
    {
        if (!IsVisible)
            return;

        _player.SetVolume((float)VolumeBar.Value / 100);
        TxtVolumePercentage.Text = (int)VolumeBar.Value + "%";
    }

    private void BtnLoop_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ToggleLoop();
        BtnLoop.Background = ViewModel.IsLooping ? _buttonActiveBrush : _buttonInactiveBrush;
    }

    private void BtnNextSong_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SkipToNextSong();
    }

    private void BtnPreviousSong_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SkipToPrevSong();
    }

    private void OnStopCalled(object? sender, EventArgs e)
    {
        TxtSongTitle.Text = _player.SongTitle;
        TxtCurrentTime.Text = _player.CurrentTime.TotalSeconds.ToMmSs();
        TxtTotalTime.Text = _player.TotalTime.TotalSeconds.ToMmSs();
        TxtArtistName.Text = string.Empty;
        SeekBar.Value = 0;
    }
}