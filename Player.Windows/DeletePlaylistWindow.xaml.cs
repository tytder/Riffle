using System.Windows;

namespace Riffle.Player.Windows;

public partial class DeletePlaylistWindow : Window
{
    public DeletePlaylistWindow()
    {
        InitializeComponent();
    }

    private void OnYesClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnNoClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}