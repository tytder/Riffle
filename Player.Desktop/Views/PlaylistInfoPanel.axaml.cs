using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.ViewModels;
using Riffle.Core.Models;
using TagLib;

namespace Player.Desktop.Views;

public partial class PlaylistInfoPanel : UserControl
{
    private readonly SolidColorBrush _buttonInactiveBrush;
    private readonly SolidColorBrush _buttonActiveBrush;
    private MainWindowViewModel _viewModel;

    public MainWindowViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            PlaylistInfo.Text = ViewModel.SelectedPlaylistInfo;
        }
    }
    
    public PlaylistInfoPanel()
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
        
        _buttonInactiveBrush = new SolidColorBrush(Colors.White);
        if (TryGetResource("ButtonInactive", ActualThemeVariant, out var inactive))
            if (inactive is SolidColorBrush inactiveBrush) _buttonInactiveBrush = inactiveBrush;
        
        _buttonActiveBrush = new SolidColorBrush(Colors.DimGray);
        if (TryGetResource("ButtonInactive", ActualThemeVariant, out var active))
            if (active is SolidColorBrush activeBrush) _buttonActiveBrush = activeBrush;
            
        BtnShuffleHeader.Background = _buttonInactiveBrush;

        if (ViewModel == null) return;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        ViewModel = vm;
    }
    
    public static FilePickerFileType AudioAll { get; } = new("Audio files")
    {
        Patterns = new[] { "*.mp3", "*.wav", "*.m4a", "*.flac", "*.opus", "*.ogg" },
    };

    private async void BtnImportSong_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        var files = await window.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                AllowMultiple = true,
                FileTypeFilter = [AudioAll],
            });

        if (files.Count == 0)
            return;

        foreach (var file in files)
        {
            var uri = file.Path.LocalPath;
            ShowSongMetadataDialog(uri);
        }
    }

    private async void ShowSongMetadataDialog(string filePath)
    {
        var file = new File.LocalFileAbstraction(filePath);
        var tagFile = File.Create(file);

        var suggestedTitle = !string.IsNullOrEmpty(tagFile.Tag.Title)
            ? tagFile.Tag.Title
            : System.IO.Path.GetFileNameWithoutExtension(filePath);

        var suggestedArtist = tagFile.Tag.Performers is { Length: > 0 }
            ? tagFile.Tag.Performers[0]
            : string.Empty;

        var metadataWindow = new SongImportData()
        {
            FilePath = System.IO.Path.GetFileName(filePath),
            TxtSongTitle = { Text = suggestedTitle },
            TxtArtistName = { Text = suggestedArtist }
        };
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        var result = await metadataWindow.ShowDialog<bool?>(window);
        if (result != true)
            return;

        var title = metadataWindow.SongTitle;
        var artist = metadataWindow.ArtistName;
        
        var duration = tagFile.Properties.Duration;
        
        var playlist = ViewModel.SelectedPlaylist.Playlist;

        var newSong = new Song(title, artist, duration, filePath);
        var newPlaylistSong = ViewModel.AddSong(newSong, playlist);

        if (ViewModel.Player.IsPlaying)
            return;

        ViewModel.PlayFrom(ViewModel.SelectedPlaylist, newPlaylistSong);
    }


    private void PlaylistContent_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SetSongGridColumnWidths();
    }

    private void SetSongGridColumnWidths()
    {
        /*if (GridView.Columns.Count < 2)
            return;*/

        const double scrollBarWidth = 18;

        var totalWidth =
            PlaylistContent.Bounds.Width
            - scrollBarWidth
            /*- GridView.Columns[0].Width
            - GridView.Columns[^1].Width*/
            - PlaylistContent.Padding.Left;

        if (totalWidth <= 0)
            return;

        //var ratios = GetPresetForWidth(totalWidth);

        /*for (var i = 0; i < GridView.Columns.Count - 2 && i < ratios.Length; i++)
        {
            var ratio = ratios[i];

            var col = GridView.Columns[i + 1];
            col.Width = ratio <= 0 ? 0 : totalWidth * ratio;
        }*/
    }
    
    private void PlaylistContent_OnMouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        if (PlaylistContent.SelectedItem is not PlaylistSong selectedSong)
            return;

        ViewModel.PlayFrom(ViewModel.SelectedPlaylist, selectedSong);
    }
    
    private void PlaylistPlayBtn_OnBtnClick(object? sender, RoutedEventArgs e)
    {
        if (!Equals(ViewModel.CurrentPlaylistPlaying, ViewModel.SelectedPlaylist))
        {
            ViewModel.PlayFrom(ViewModel.SelectedPlaylist);
            return;
        }

        ViewModel.Player.TogglePlaying();
    }
}