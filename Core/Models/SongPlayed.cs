namespace Riffle.Core.Models;

public class SongPlayed
{
    public required Song Song { get; set; }
    public DateTime PlayedAt { get; set; }
}