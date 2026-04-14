using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Player.Desktop.Views;

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