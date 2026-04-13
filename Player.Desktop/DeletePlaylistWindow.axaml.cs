using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Player.Desktop;

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