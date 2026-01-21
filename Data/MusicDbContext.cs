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
    
    public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();

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
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsAllSongs).HasDefaultValue(false);
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(ps => new { ps.PlaylistId, ps.SongId });

            entity.HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistItems)
                .HasForeignKey(ps => ps.PlaylistId);

            entity.HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistItems)
                .HasForeignKey(ps => ps.SongId);

            entity.Property(ps => ps.DateAdded)
                .HasColumnType("TEXT")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });
    }
}