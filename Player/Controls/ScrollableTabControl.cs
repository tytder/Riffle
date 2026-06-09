using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace Player.Controls;

public class ScrollableTabControl : TabControl
{
    protected override Type StyleKeyOverride => typeof(TabControl);
}