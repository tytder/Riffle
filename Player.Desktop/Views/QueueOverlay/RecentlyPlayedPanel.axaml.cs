using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.ViewModels;

namespace Player.Desktop.Views;

public partial class RecentlyPlayedPanel : UserControl
{
    private MainWindowViewModel _viewModel;

    public MainWindowViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel is { Player: not null }) _viewModel.Player.TrackLoaded -= PlayerOnTrackLoaded;
            _viewModel = value;
            _viewModel.Player.TrackLoaded += PlayerOnTrackLoaded;
        }
    }
    
    public RecentlyPlayedPanel()
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
    
        ViewModel.Player.TrackLoaded += PlayerOnTrackLoaded;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        ViewModel = vm;
    }
    
    private void PlayerOnTrackLoaded(object? sender, EventArgs e)
    {
        RecentlyPlayedListView.ItemsSource = ViewModel.RecentlyPlayed;
    }
}