using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Riffle.Core.Services;
using Riffle.Data;
using Riffle.Player.Windows.Services;
using Riffle.Player.Windows.ViewModels;

namespace Riffle.Player.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        /*base.OnStartup(e);

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
        */
        
        base.OnStartup(e);

        var services = new ServiceCollection();
    
        // Db options first
        services.AddDbContext<MusicDbContext>(options =>
            options.UseSqlite("Data Source=music.db"));

        services.AddSingleton<ILibraryManager, LibraryManager>();
        services.AddSingleton<NAudioAudioPlayer>(); 
        services.AddSingleton<PlaybackManager>();
        
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<SidebarViewModel>();
        services.AddTransient<SongsViewModel>();
    
        ServiceProvider = services.BuildServiceProvider();
    
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MusicDbContext>();
            db.Database.Migrate();

            var library = scope.ServiceProvider.GetRequiredService<ILibraryManager>();
            // If InitializeAsync exists:
            library.Initialize();  // Creates All Songs
        }
        
        // ViewModels via DI
        var mainVm = ServiceProvider.GetService<MainWindowViewModel>();
        var player = ServiceProvider.GetService<NAudioAudioPlayer>();
        var mainWindow = new MainWindow(mainVm, player);  
        mainWindow.Show();
    }
}

