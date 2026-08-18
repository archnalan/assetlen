namespace assetlen.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // 360px is the charter's floor (CLAUDE.md §3). The desktop window opens
        // at reading width rather than maximised so the folio layout is what a
        // reviewer sees first.
        var window = new Window(new MainPage()) { Title = "ASSETLEN" };

#if WINDOWS
        window.Width = 1280;
        window.Height = 900;
        window.MinimumWidth = 360;
        window.MinimumHeight = 560;
#endif
        return window;
    }
}
