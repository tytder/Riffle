using Player.Desktop.Models;
using Riffle.Core.Services;
using Riffle.Data;

namespace Player.Desktop.ViewModels;

public class DesignerMainWindowViewModel : MainWindowViewModel
{
    public DesignerMainWindowViewModel(ILibraryManager fakeLibraryManager) : 
        base(
            fakeLibraryManager, 
            new DummyAudioPlayer(),
            new SidebarViewModel(fakeLibraryManager),
            new SongsViewModel(fakeLibraryManager))
    {
        fakeLibraryManager.Initialize();
    }

    public DesignerMainWindowViewModel() : this(new FakeLibraryManager())
    {
        
    }
}