namespace Riffle.Core.Interfaces;

public class PlayingStateEventArgs : EventArgs
{
    public bool IsPlaying { get; }

    public PlayingStateEventArgs(bool isPlaying)
    {
        IsPlaying = isPlaying;
    }
}