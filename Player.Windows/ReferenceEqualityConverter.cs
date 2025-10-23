using System;
using System.Globalization;
using System.Windows.Data;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class ReferenceEqualityConverter : IMultiValueConverter /*IValueConverter*/
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        
        if (values.Length == 2)
        {
            if (values[0] is not PlaylistViewModel openPlaylist) return false;
            if (values[1] is not PlaylistViewModel playingPlaylist) return false;

            return Equals(openPlaylist, playingPlaylist);
        }

        if (values.Length == 4)
        {
            if (values[0] is not Song thisRowSong) return false;
            if (values[1] is not Song currentSong) return false;
            if (values[2] is not PlaylistViewModel openPlaylist) return false;
            if (values[3] is not PlaylistViewModel playingPlaylist) return false;

            // Highlight if the song matches AND the playlist currently being viewed is the playlist thats playing
            return Equals(thisRowSong, currentSong) && Equals(openPlaylist, playingPlaylist);
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
    
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var song = value as Song;
        var currentSong = parameter as Song;
        return song != null && currentSong != null && song.Equals(currentSong);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}