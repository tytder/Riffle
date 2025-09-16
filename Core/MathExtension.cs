using System;

namespace Riffle.Core;

public static class MathExtension
{
    public static string ToMmSs(this double d1) 
    {
        int totalSeconds = (int)d1;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
}