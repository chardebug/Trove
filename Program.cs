using Avalonia;
using Avalonia.Media;
using System;
using LibVLCSharp.Shared;

namespace Trove;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread] 
    public static void Main(string[] args)
    {
        Core.Initialize();   // loads the native VLC libraries; must run before any LibVLC/MediaPlayer is created

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .ConfigureFonts(fontManager =>
            {
                fontManager.AddFontCollection(new TroveFonts());
            })
            .WithInterFont()
            .LogToTrace();
}
