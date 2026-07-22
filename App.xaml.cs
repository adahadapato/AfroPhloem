namespace AfroPhloem;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            // Sensible default desktop window size; MAUI still fully adapts
            // to phones/tablets since Shell/FlexLayout/Grid are all responsive.
            Width = 1200,
            Height = 800,
            MinimumWidth = 380,
            MinimumHeight = 640
        };
        return window;
    }
}
