using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Riffle.Core.Audio;

namespace Riffle.Player.Windows;

public class ReferenceEqualityConverter : IMultiValueConverter /*IValueConverter*/
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        //if (values[0] is Song && values[1] is Song)
            return Equals(values[0], values[1]);
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