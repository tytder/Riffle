using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Player.Desktop;

public partial class SongImportData : Window
{
    public string SongTitle { get; private set; } = null!;
    public string ArtistName { get; private set; } = null!;
    public TimeSpan Duration { get; private set; }

    private string _filePath = null!;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            TxtFilePath.Text = _filePath;
            //TxtSongTitle.Text = _filePath.Split('\\')[^1][..^4]; // splits the file path and gets the name of the file, then removes the file extension
        }
    }

    public SongImportData()
    {
        InitializeComponent();
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        SongTitle = TxtSongTitle.Text ?? _filePath.Split('\\')[^1][..^4]; // splits the file path and gets the name of the file, then removes the file extension;
        ArtistName = TxtArtistName.Text ?? "";
        Close(true);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}