namespace Riffle.Core.Utilities;

public static class Extensions
{
    public static string ToMmSs(this double d1) 
    {
        int totalSeconds = (int)d1;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
    
    public static string ToFriendlyAge(this DateTime dateAddedUtc)
    {
        var now = DateTime.UtcNow;
        var delta = now - dateAddedUtc;

        if (delta < TimeSpan.Zero)
            delta = TimeSpan.Zero; // future-safe

        if (delta < TimeSpan.FromMinutes(1))
        {
            var seconds = (int)delta.TotalSeconds;
            if (seconds <= 1) return "just now";
            return $"{seconds} seconds ago";
        }

        if (delta < TimeSpan.FromHours(1))
        {
            var minutes = (int)delta.TotalMinutes;
            return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
        }

        if (delta < TimeSpan.FromDays(1))
        {
            var hours = (int)delta.TotalHours;
            return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
        }

        if (delta < TimeSpan.FromDays(7))
        {
            var days = (int)delta.TotalDays;
            return days == 1 ? "1 day ago" : $"{days} days ago";
        }

        if (delta < TimeSpan.FromDays(28)) // 4 weeks
        {
            var weeks = (int)(delta.TotalDays / 7);
            return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
        }

        // 4+ weeks ago: show date
        // adjust format to your liking / culture
        return dateAddedUtc.ToLocalTime().ToString("dd/MMM/yyyy");
    }

}