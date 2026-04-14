using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Player.Desktop.Views;

public partial class DeletePlaylistWindow : Window
{
    public DeletePlaylistWindow()
    {
        InitializeComponent();
    }

    private void OnYesClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        Close(true);
    }

    private void OnNoClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}