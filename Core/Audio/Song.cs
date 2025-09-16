using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Riffle.Core.Audio;

public class Song //: INotifyPropertyChanged
{
    public Song(string title, string artist, TimeSpan duration, string filePath)
    {
        Title = title;
        Artist = artist;
        Duration = duration;
        FilePath = filePath;
        Id = Guid.NewGuid();
    }

    public string Title { get; }
    public string Artist { get; }
    public TimeSpan Duration { get; }
    public string DurationDisplay => Duration.TotalSeconds.ToMmSs();
    public string FilePath { get; }
    /*private bool _isPlaying; //TODO: set playing from MainWindow.xaml.cs
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            if (_isPlaying != value)
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
    }*/
    public Guid Id { get; }
    
    public bool IsAvailable => File.Exists(FilePath);
    
    public override bool Equals(object? obj)
    {
        return obj is Song other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
    
    // TODO: also remove
    /*public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }*/
}