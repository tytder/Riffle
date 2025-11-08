using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Riffle.Data;
using Riffle.Player.Windows.Services;

namespace Riffle.Player.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public MusicService MusicService { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = new DbContextOptionsBuilder<MusicDbContext>()
            .UseSqlite("Data Source=music.db")
            .Options;

        // Create a single shared service instance
        MusicService = new MusicService(options);
        
        try
        {
            using var db = new MusicDbContext(options);
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed:\n{ex.Message}");
        }

        var mainWindow = new MainWindow(MusicService);
        mainWindow.Show();
    }
}

