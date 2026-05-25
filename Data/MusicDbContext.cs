using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Riffle.Core.Models;

namespace Riffle.Data;

public class MusicDbContext : DbContext
{
    public const string AppName = "RifflePlayer";
    
    public static string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName,
        "music.db"
    );
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    
    public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();
    public DbSet<SongPlayed> SongHistory => Set<SongPlayed>();

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
            
            entity.Ignore(s => s.IsAvailable);
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsAllSongs).HasDefaultValue(false);
            entity.Property(e => e.LastPlayed)
                .HasColumnType("TEXT")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistItems)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistItems)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ps => ps.DateAdded)
                .HasColumnType("TEXT")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
            
            entity.Ignore(ps => ps.HowLongAgo);
        });
        
        modelBuilder.Entity<SongPlayed>(entity =>
        {
            entity.ToTable("SongPlayed");
            entity.HasKey(sp => sp.Id);
            entity.Property(sp => sp.Id).ValueGeneratedOnAdd();
            entity.Property(sp => sp.PlayedAt).IsRequired();
            entity.Property(sp => sp.PlayedFromName).HasMaxLength(256);
            entity.Property(sp => sp.SongName).HasMaxLength(256);
            entity.Property(sp => sp.ArtistName).HasMaxLength(256);

            entity.HasOne(sp => sp.Song)
                .WithMany()
                .HasForeignKey(sp => sp.SongId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(sp => sp.Playlist)
                .WithMany()
                .HasForeignKey(sp => sp.PlaylistId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(sp => sp.PlaylistSong)
                .WithMany()
                .HasForeignKey(sp => sp.PlaylistSongId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Ignore(sp => sp.HowLongAgo);
        });
    }
}