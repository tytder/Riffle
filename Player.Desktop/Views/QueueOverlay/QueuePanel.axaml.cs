using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.ViewModels;

namespace Player.Desktop.Views;

public partial class QueuePanel : UserControl
{
    private MainWindowViewModel _viewModel;

    public MainWindowViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            if (ViewModel != null && ViewModel.Player != null ) ViewModel.Player.TrackLoaded -= PlayerOnTrackLoaded;
            _viewModel = value;
            ViewModel!.Player!.TrackLoaded += PlayerOnTrackLoaded;
        }
    }

    public QueuePanel()
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
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        ViewModel = vm;
    }
    
    private void PlayerOnTrackLoaded(object? sender, EventArgs e)
    {
        QueueListView.ItemsSource = ViewModel.TotalQueue;
    }
    
    private void ClearQueueClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ClearUserQueue();
    }
    
    private void GoToCurrentSongPlaylist(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentSong == null)
            return;

        var songPlaylist = ViewModel.GetPlaylistModel(ViewModel.CurrentSong);
        if (songPlaylist != null) ViewModel.OpenPlaylist(songPlaylist);
    }

    private void GoToCurrentPlaylistPlaying(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPlaylistPlaying == null)
            return;

        ViewModel.OpenPlaylist();
    }
}