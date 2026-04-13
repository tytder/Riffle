using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Riffle.Core.Models;

namespace Player.Desktop.Converters;

public class ReferenceEqualityConverter : IMultiValueConverter /*IValueConverter*/
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return false;
        
        if (values[0] is Guid id1 && values[1] is Guid id2)
            return id1.Equals(id2);

        return false;
        
        /*if (values[0] is PlaylistViewModel openPlaylist
            && values[1] is PlaylistViewModel playingPlaylist)
        {
            return Equals(openPlaylist, playingPlaylist);
        }
            
        if (values[0] is PlaylistSong thisRowSong
            && values[1] is PlaylistSong currentSong)
        {
            return Equals(thisRowSong, currentSong);
        }*/

        /*if (values.Length == 4)
        {
            if (values[0] is not PlaylistSong thisRowSong) return false;
            if (values[1] is not PlaylistSong currentSong) return false;
            if (values[2] is not PlaylistViewModel openPlaylist) return false;
            if (values[3] is not PlaylistViewModel playingPlaylist) return false;

            // Highlight if the song matches AND the playlist currently being viewed is the playlist thats playing
            return Equals(thisRowSong, currentSong) && Equals(openPlaylist, playingPlaylist);
        }*/

        //return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
    
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var song = value as PlaylistSong;
        var currentSong = parameter as PlaylistSong;
        return song != null && currentSong != null && song.Equals(currentSong);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}