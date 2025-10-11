using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Riffle.Core.Audio;

# nullable enable
public class Song
{
    public Song(string title, string? artist, TimeSpan duration, string filePath)
    {
        Title = title;
        Artist = artist;
        Duration = duration;
        FilePath = filePath;
    }

    public string Title { get; private set; }
    public string? Artist { get; private set; }
    public TimeSpan Duration { get; private set; }
    public string DurationDisplay => Duration.TotalSeconds.ToMmSs();
    public string FilePath { get;  private set; }
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public bool IsAvailable => File.Exists(FilePath);
    
    public override bool Equals(object? obj)
    {
        return obj is Song other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
# nullable disable