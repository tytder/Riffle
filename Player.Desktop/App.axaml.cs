#nullable enable
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Player.Desktop.Models;
using Player.Desktop.ViewModels;
using Riffle.Core.Interfaces;
using Riffle.Core.Services;
using Riffle.Data;

namespace Player.Desktop;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // --- DI SETUP ---
        var services = new ServiceCollection();
        
        Directory.CreateDirectory(
            Path.GetDirectoryName(MusicDbContext.DbPath)!
        );

        services.AddDbContext<MusicDbContext>(options =>
            options.UseSqlite($"Data Source={MusicDbContext.DbPath}"));

        services.AddSingleton<ILibraryManager, LibraryManager>();
        services.AddSingleton<VlcAudioPlayer>(); // switched player
        services.AddSingleton<IAudioPlayer>(sp => sp.GetRequiredService<VlcAudioPlayer>());
        services.AddSingleton<PlaybackManager>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<SidebarViewModel>();
        services.AddTransient<SongsViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        // --- INIT DATABASE + LIBRARY ---
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MusicDbContext>();
            
            try
            {
                db.Database.Migrate();

                var library = scope.ServiceProvider.GetRequiredService<ILibraryManager>();
                library.Initialize();
            }
            catch (Exception ex)
            {
                // Avalonia has no built-in MessageBox
                Console.WriteLine($"Database initialization failed: {ex}");
            }
        }

        // --- WINDOW SETUP ---
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = ServiceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow(mainVm);
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}