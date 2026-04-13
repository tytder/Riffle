using System.Windows;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Player.Desktop;

public partial class NewPlaylistWindow : Window
{
    public string PlaylistName { get; set; }
    
    public NewPlaylistWindow()
    {
        InitializeComponent();
    }

    private void OnOkClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        PlaylistName = TxtPlaylistName.Text;
        Close(true);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}