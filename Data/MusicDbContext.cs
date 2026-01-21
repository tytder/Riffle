using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Riffle.Core.Models;

namespace Riffle.Data;

public class MusicDbContext : DbContext
{
    public static string AppName = Assembly.GetEntryAssembly()!.GetName().Name!;
    
    public static string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName,
        "music.db"
    );
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Playlist> Playlists => Set<Playlist>();

    public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.AddedAt)
                .HasColumnType("TEXT")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasMany(p => p.PlaylistItems).WithMany();
        });
    }
}