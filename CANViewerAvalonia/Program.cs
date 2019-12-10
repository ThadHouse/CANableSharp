using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using CANableSharp;

namespace CANViewerAvalonia
{
    class Program
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            CandleInvoke.Initialize();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

    }
}
