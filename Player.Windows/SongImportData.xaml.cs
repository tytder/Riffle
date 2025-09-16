using System;
using System.Windows;
using TagLib;


namespace Riffle.Player.Windows;

public partial class SongImportData : Window
{
    public string SongTitle { get; private set; }
    public string ArtistName { get; private set; }
    public TimeSpan Duration { get; private set; }

    private string _filePath;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            TxtFilePath.Text = _filePath;
            TxtSongTitle.Text = _filePath.Split('\\')[^1][..^4]; // splits the file path and gets the name of the file, then removes the file extension
        }
    }

    public SongImportData()
    {
        InitializeComponent();
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        SongTitle = TxtSongTitle.Text;
        ArtistName = TxtArtistName.Text;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    

    private TimeSpan GetDuration(string filePath)
    {
        var file = TagLib.File.Create(filePath);
        return file.Properties.Duration;
    }

}