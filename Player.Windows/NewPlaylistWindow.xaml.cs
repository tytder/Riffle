using System.Windows;

namespace Riffle.Player.Windows;

public partial class NewPlaylistWindow : Window
{
    public string PlaylistName { get; set; }
    
    public NewPlaylistWindow()
    {
        InitializeComponent();
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        PlaylistName = TxtPlaylistName.Text;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}