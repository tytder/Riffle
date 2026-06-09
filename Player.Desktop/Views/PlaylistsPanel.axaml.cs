using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.ViewModels;

namespace Player.Desktop.Views;

public partial class PlaylistsPanel : UserControl
{
    private MainWindowViewModel _viewModel;

    public MainWindowViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            NewViewModelSet();
        }
    }

    private void NewViewModelSet()
    {
        PlaylistList.SelectedIndex = 0;
        PlaylistList.ItemsSource = ViewModel.SidebarViewModel.Playlists;
    }

    public PlaylistsPanel()
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

    private void PlaylistList_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        /*const double scrollBarWidth = 18;
        var totalWidth = PlaylistContent.Bounds.Width - scrollBarWidth;
        if (totalWidth > 0) PlaylistList.Width = totalWidth * 4 / 12;*/
        /*if (PlaylistView.Columns.Count > 0)
        {
            PlaylistView.Columns[0].Width = totalWidth * 4 / 12;
        }*/
    }

    /*
    private void PlaylistList_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        ViewModel.OpenPlaylist(selectedVm);
    }*/
    private void PlaylistList_OnPressed(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not Button button) return;
        var buttonContext = button.DataContext;
        if (buttonContext is not PlaylistViewModel selectedVm)
            return;
        
        PlaylistList.SelectedItem = selectedVm;
        ViewModel.OpenPlaylist(selectedVm);
    }

    private void PlaylistList_OnMouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        ViewModel.SongsViewModel.RefreshSongs();
        ViewModel.PlayFrom(selectedVm);
    }

    private async void AddPlaylist_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        
        var playlistWindow = new NewPlaylistWindow();
        var result = await playlistWindow.ShowDialog<bool>(window);
        if (result)
        {
            //PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
            Dispatcher.UIThread.Post(() =>
            {
                PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
            });
        }
        /*playlistWindow.ShowDialog<bool?>(this).ContinueWith(t =>
        {
            if (t.Result == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    PlaylistList.SelectedItem = ViewModel.CreatePlaylist(playlistWindow.PlaylistName);
                });
            }
        });*/
    }

    private async void RemovePlaylist_Click(object? sender, RoutedEventArgs e)
    {
        if (PlaylistList.SelectedItem is not PlaylistViewModel selectedVm)
            return;

        if (TopLevel.GetTopLevel(this) is not Window window) return;
        
        var deletePlaylistWindow = new DeletePlaylistWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var result = await deletePlaylistWindow.ShowDialog<bool>(window);
        if (result)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ViewModel.DeletePlaylist(selectedVm);
                ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
            });
            //ViewModel.DeletePlaylist(selectedVm);
            //ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
        }
        /*deletePlaylistWindow.ShowDialog<bool?>(this).ContinueWith(t =>
        {
            if (t.Result == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ViewModel.DeletePlaylist(selectedVm);
                    ViewModel.SelectedPlaylist = ViewModel.GetAllSongsPlaylist();
                });
            }
        });*/
    }

}